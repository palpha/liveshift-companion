#include <windows.h>
#include <dxgi.h>
#include <cstdio>

int main()
{
    IDXGIFactory* pFactory = nullptr;
    HRESULT hr = CreateDXGIFactory(__uuidof(IDXGIFactory), (void**)&pFactory);
    if (FAILED(hr)) {
        printf("CreateDXGIFactory failed: 0x%08X\n", hr);
        return 1;
    }

    IDXGIAdapter* pAdapter = nullptr;
    hr = pFactory->EnumAdapters(0, &pAdapter);
    if (FAILED(hr)) {
        printf("EnumAdapters(0) failed: 0x%08X\n", hr);
    }
    else {
        printf("Got adapter!\n");
        pAdapter->Release();
    }

    pFactory->Release();
    return 0;
}