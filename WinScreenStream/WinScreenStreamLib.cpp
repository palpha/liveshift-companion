#include "pch.h"
#include "WinScreenStreamLib.h"
#include <windows.h>
#include <dxgi1_2.h>
#include <d3d11.h>
#include <wrl/client.h>
#include <thread>
#include <atomic>
#include <vector>
#include <cstdio>

using Microsoft::WRL::ComPtr;

struct DisplayContext {
    int id;
    ComPtr<IDXGIOutput> output;
    int width;
    int height;
    char name[128];
    bool isPrimary;
};

// Global or static storage for enumerated displays
static std::vector<DisplayContext> gDisplays;

// Thread control
static std::thread gCaptureThread;
static std::atomic<bool> gCaptureRun{ false };
static int gCurrentDisplayId = -1;
static CaptureFrameCallback gCallback = nullptr;
static void* gCallbackUserContext = nullptr;

// Helper for releasing COM
template<typename T>
void SafeRelease(T*& ptr) {
    if (ptr) {
        ptr->Release();
        ptr = nullptr;
    }
}

// ------------------------------------------------------
// GetActiveDisplays
// ------------------------------------------------------
int GetActiveDisplays(DisplayInfo* infos, int maxCount)
{
    gDisplays.clear();

    ComPtr<IDXGIFactory1> factory;
    if (FAILED(CreateDXGIFactory1(__uuidof(IDXGIFactory1), (void**)&factory))) {
        printf("Failed to create DXGI factory\n");
        return 0;
    }

    UINT adapterIndex = 0;
    ComPtr<IDXGIAdapter1> adapter;
    int displayCount = 0;

    while (factory->EnumAdapters1(adapterIndex, &adapter) != DXGI_ERROR_NOT_FOUND) {
        UINT outputIndex = 0;
        ComPtr<IDXGIOutput> output;
        while (adapter->EnumOutputs(outputIndex, &output) != DXGI_ERROR_NOT_FOUND) {
            DXGI_OUTPUT_DESC desc;
            output->GetDesc(&desc);

            if (desc.AttachedToDesktop) {
                DisplayContext ctx;
                ctx.id = displayCount;
                ctx.output = output;
                ctx.width = desc.DesktopCoordinates.right - desc.DesktopCoordinates.left;
                ctx.height = desc.DesktopCoordinates.bottom - desc.DesktopCoordinates.top;
                WideCharToMultiByte(CP_ACP, 0, desc.DeviceName, -1, ctx.name, 128, NULL, NULL);
                ctx.name[127] = 0; // Ensure null termination

                // Identify primary display
                ctx.isPrimary = (desc.DesktopCoordinates.left == 0 && desc.DesktopCoordinates.top == 0);

                gDisplays.push_back(ctx);

                if (displayCount < maxCount && infos) {
                    infos[displayCount].id = displayCount;
                    strcpy_s(infos[displayCount].name, ctx.name);
                    infos[displayCount].width = ctx.width;
                    infos[displayCount].height = ctx.height;
                    infos[displayCount].isPrimary = ctx.isPrimary;
                }

                printf("Display %d: %s (%dx%d) %s\n",
                    displayCount, ctx.name, ctx.width, ctx.height,
                    ctx.isPrimary ? "[PRIMARY]" : "");

                displayCount++;
            }
            output.Reset();
            outputIndex++;
        }
        adapter.Reset();
        adapterIndex++;
    }

    return displayCount;
}

// ------------------------------------------------------
// Capture Thread (Desktop Duplication)
// ------------------------------------------------------
static void CaptureThread(int displayId)
{
    // Grab display info
    if (displayId < 0 || displayId >= (int)gDisplays.size()) {
        printf("Invalid displayId: %d\n", displayId);
        return;
    }
    DisplayContext& ctx = gDisplays[displayId];

    // Create D3D11 device
    ComPtr<ID3D11Device> d3dDevice;
    ComPtr<ID3D11DeviceContext> d3dContext;
    HRESULT hr = D3D11CreateDevice(
        nullptr,
        D3D_DRIVER_TYPE_HARDWARE,
        nullptr,
        0, // flags
        nullptr, 0, // feature levels
        D3D11_SDK_VERSION,
        &d3dDevice,
        nullptr,
        &d3dContext
    );
    if (FAILED(hr)) {
        printf("D3D11CreateDevice failed: 0x%08X\n", hr);
        return;
    }

    // Query for IDXGIDevice
    ComPtr<IDXGIDevice> dxgiDevice;
    hr = d3dDevice.As(&dxgiDevice);
    if (FAILED(hr)) {
        printf("As IDXGIDevice failed: 0x%08X\n", hr);
        return;
    }

    // Get the output to duplicate
    ComPtr<IDXGIOutput1> output1;
    hr = ctx.output.As(&output1);
    if (FAILED(hr)) {
        printf("As IDXGIOutput1 failed: 0x%08X\n", hr);
        return;
    }

    // Desktop Duplication interface
    ComPtr<IDXGIOutputDuplication> duplication;
    hr = output1->DuplicateOutput(d3dDevice.Get(), &duplication);
    if (FAILED(hr)) {
        printf("DuplicateOutput failed: 0x%08X\n", hr);
        return;
    }

    // Frame variables
    DXGI_OUTDUPL_FRAME_INFO frameInfo = {};
    ComPtr<IDXGIResource> desktopResource;
    ComPtr<ID3D11Texture2D> acquiredTex;

    printf("Capture thread started for display #%d\n", displayId);

    while (gCaptureRun) {
        // Acquire next frame (blocking or with a short wait)
        desktopResource.Reset();
        acquiredTex.Reset();

        hr = duplication->AcquireNextFrame(100, // 100ms timeout
            &frameInfo, &desktopResource);
        if (hr == DXGI_ERROR_WAIT_TIMEOUT) {
            // no new frame - keep going
            continue;
        }
        else if (FAILED(hr)) {
            // Lost access or other errors
            printf("AcquireNextFrame failed: 0x%08X\n", hr);
            break;
        }

        // Query for ID3D11Texture2D
        hr = desktopResource.As(&acquiredTex);
        if (SUCCEEDED(hr)) {
            // Map/copy the texture to CPU memory
            D3D11_TEXTURE2D_DESC desc;
            acquiredTex->GetDesc(&desc);

            // Create a staging texture
            desc.Usage = D3D11_USAGE_STAGING;
            desc.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
            desc.BindFlags = 0;
            desc.MiscFlags = 0;

            ComPtr<ID3D11Texture2D> stagingTex;
            hr = d3dDevice->CreateTexture2D(&desc, nullptr, &stagingTex);
            if (SUCCEEDED(hr)) {
                // Copy frame
                d3dContext->CopyResource(stagingTex.Get(), acquiredTex.Get());

                // Map and read
                D3D11_MAPPED_SUBRESOURCE map;
                hr = d3dContext->Map(stagingTex.Get(), 0, D3D11_MAP_READ, 0, &map);
                if (SUCCEEDED(hr)) {
                    // Pixel pointer
                    auto* pixels = reinterpret_cast<unsigned char*>(map.pData);
                    int pitch = map.RowPitch; // in bytes

                    // Call user callback
                    if (gCallback) {
                        gCallback(pixels, ctx.width, ctx.height, gCallbackUserContext);
                    }

                    d3dContext->Unmap(stagingTex.Get(), 0);
                }
            }
        }

        // Release frame
        duplication->ReleaseFrame();
    }

    printf("Capture thread ending for display #%d\n", displayId);
}

// ------------------------------------------------------
// StartCapture
// ------------------------------------------------------
int StartCapture(int displayId, CaptureFrameCallback callback, void* userContext)
{
    if (displayId < 0 || displayId >= (int)gDisplays.size()) {
        printf("Invalid displayId in StartCapture: %d\n", displayId);
        return -1;
    }

    if (gCaptureRun) {
        // Already capturing
        return 0;
    }

    gCaptureRun = true;
    gCurrentDisplayId = displayId;
    gCallback = callback;
    gCallbackUserContext = userContext;
    gCaptureThread = std::thread(CaptureThread, displayId);

    return 0; // success
}

// ------------------------------------------------------
// StopCapture
// ------------------------------------------------------
void StopCapture()
{
    if (!gCaptureRun) return;

    gCaptureRun = false;
    if (gCaptureThread.joinable()) {
        gCaptureThread.join();
    }
    gCurrentDisplayId = -1;
    gCallback = nullptr;
    gCallbackUserContext = nullptr;
}