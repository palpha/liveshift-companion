using System.Diagnostics;
using System.Runtime.InteropServices;
using static Core.Image.DesktopDuplicator;

namespace Core.Image;

using System;
using System.Runtime.InteropServices;
using System.Security;

// ----------------------------------------------------------------
// P/Invoke for D3D11 and DXGI
// ----------------------------------------------------------------

internal static class D3D11
{
    private const string D3D11_DLL = "d3d11.dll";
    private const string DXGI_DLL = "dxgi.dll";

    [DllImport(D3D11_DLL, CallingConvention = CallingConvention.StdCall)]
    public static extern int D3D11CreateDevice(
        IntPtr pAdapter,
        D3D_DRIVER_TYPE DriverType,
        IntPtr Software,
        uint Flags,
        IntPtr pFeatureLevels, // for simplicity, pass IntPtr.Zero
        uint FeatureLevels, // count of levels in pFeatureLevels
        uint SDKVersion,
        out IntPtr ppDevice, // ID3D11Device**
        out IntPtr pFeatureLevel, // D3D_FEATURE_LEVEL*
        out IntPtr ppImmediateContext // ID3D11DeviceContext**
    );

    [DllImport(DXGI_DLL, CallingConvention = CallingConvention.StdCall)]
    public static extern int CreateDXGIFactory(ref Guid riid, out IntPtr ppFactory);

    [DllImport(DXGI_DLL, CallingConvention = CallingConvention.StdCall)]
    public static extern int CreateDXGIFactory2(uint Flags, ref Guid riid, out IntPtr ppFactory);
}

internal enum D3D_DRIVER_TYPE
{
    UNKNOWN = 0,
    HARDWARE = 1,

    // ...
    WARP = 5,
}

// ----------------------------------------------------------------
// DXGI interfaces
// ----------------------------------------------------------------

// IDXGIFactory (DXGI 1.0)
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("7B7166EC-21C7-44AE-B21A-C9AE321AE369")] // IID_IDXGIFactory
internal interface IDXGIFactory
{
    [PreserveSig]
    int SetPrivateData(ref Guid Name, int DataSize, IntPtr pData);

    [PreserveSig]
    int SetPrivateDataInterface(ref Guid Name, IntPtr pUnknown);

    [PreserveSig]
    int GetPrivateData(ref Guid Name, ref int pDataSize, IntPtr pData);

    [PreserveSig]
    int GetParent(ref Guid riid, out IntPtr ppParent);

    [PreserveSig]
    int EnumAdapters(uint adapterIndex, out IntPtr ppAdapter);

    [PreserveSig]
    int MakeWindowAssociation(IntPtr WindowHandle, uint Flags);

    [PreserveSig]
    int GetWindowAssociation(out IntPtr pWindowHandle);

    [PreserveSig]
    int CreateSwapChain(IntPtr pDevice, IntPtr pDesc, out IntPtr ppSwapChain);

    [PreserveSig]
    int CreateSoftwareAdapter(IntPtr Module, out IntPtr ppAdapter);
}

// IDXGIFactory1 (DXGI 1.1)
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("770aae78-f26f-4dba-a829-253c83d1b387")] // IID_IDXGIFactory1
internal interface IDXGIFactory1 : IDXGIFactory
{
    [PreserveSig]
    int EnumAdapters1(uint Adapter, out IntPtr ppAdapter);

    [PreserveSig]
    bool IsCurrent();
}

// IDXGIFactory2 (DXGI 1.2)
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("50c83a1c-e072-4c48-87b0-3630fa36a6d0")] // IID_IDXGIFactory2
internal interface IDXGIFactory2 : IDXGIFactory1
{
    [PreserveSig]
    bool IsWindowedStereoEnabled();

    [PreserveSig]
    int CreateSwapChainForHwnd(
        IntPtr pDevice,  // IUnknown*
        IntPtr hWnd,     // HWND (use IntPtr)
        IntPtr pDesc,    // DXGI_SWAP_CHAIN_DESC1*
        IntPtr pFullscreenDesc,  // DXGI_SWAP_CHAIN_FULLSCREEN_DESC*
        IntPtr pRestrictToOutput, // IDXGIOutput*
        out IntPtr ppSwapChain // IDXGISwapChain1**
    );

    [PreserveSig]
    int CreateSwapChainForCoreWindow(
        IntPtr pDevice, // IUnknown*
        IntPtr pWindow, // IUnknown*
        IntPtr pDesc, // DXGI_SWAP_CHAIN_DESC1*
        IntPtr pRestrictToOutput, // IDXGIOutput*
        out IntPtr ppSwapChain // IDXGISwapChain1**
    );

    [PreserveSig]
    int GetSharedResourceAdapterLuid(
        IntPtr hResource, // HANDLE
        out LUID pLuid
    );

    [PreserveSig]
    int RegisterStereoStatusWindow(
        IntPtr windowHandle, // HWND
        uint msg, // UINT
        out uint cookie // DWORD*
    );

    [PreserveSig]
    int RegisterStereoStatusEvent(
        IntPtr hEvent, // HANDLE
        out uint cookie // DWORD*
    );

    [PreserveSig]
    void UnregisterStereoStatus(uint cookie);

    [PreserveSig]
    int RegisterOcclusionStatusWindow(
        IntPtr windowHandle, // HWND
        uint msg, // UINT
        out uint cookie // DWORD*
    );

    [PreserveSig]
    int RegisterOcclusionStatusEvent(
        IntPtr hEvent, // HANDLE
        out uint cookie // DWORD*
    );

    [PreserveSig]
    void UnregisterOcclusionStatus(uint cookie);

    [PreserveSig]
    int CreateSwapChainForComposition(
        IntPtr pDevice, // IUnknown*
        IntPtr pDesc, // DXGI_SWAP_CHAIN_DESC1*
        IntPtr pRestrictToOutput, // IDXGIOutput*
        out IntPtr ppSwapChain // IDXGISwapChain1**
    );
}

// IDXGIFactory3 (DXGI 1.3)
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("25483823-cd46-4c7d-86ca-47aa95b837bd")] // IID_IDXGIFactory3
internal interface IDXGIFactory3 : IDXGIFactory2
{
    [PreserveSig]
    uint GetCreationFlags();
}

// IDXGIFactory4 (DXGI 1.4)
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("1bc6ea02-ef36-464f-bf0c-21ca39e5168a")] // IID_IDXGIFactory4
internal interface IDXGIFactory4 : IDXGIFactory3
{
    [PreserveSig]
    int EnumAdapterByLuid(ref LUID AdapterLuid, ref Guid riid, out IntPtr ppvAdapter);

    [PreserveSig]
    int EnumWarpAdapter(ref Guid riid, out IntPtr ppvAdapter);
}

// IDXGIFactory5 (DXGI 1.6)
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("7632e1f5-ee65-4dca-87fd-84cd75f8838d")] // IID_IDXGIFactory5
internal interface IDXGIFactory5 : IDXGIFactory4
{
    [PreserveSig]
    int CheckFeatureSupport(int Feature, IntPtr pFeatureSupportData, uint FeatureSupportDataSize);
}

// IDXGIFactory6 (DXGI 1.7)
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("c1b6694f-ff09-44a9-b03c-77900a0a1d17")] // IID_IDXGIFactory6
internal interface IDXGIFactory6 : IDXGIFactory5
{
    [PreserveSig]
    int EnumAdapterByGpuPreference(
        uint Adapter,
        uint GpuPreference, // DXGI_GPU_PREFERENCE enum
        ref Guid riid,
        out IntPtr ppvAdapter
    );
}

// IDXGIFactory7 (DXGI 1.8)
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("a4966eed-76db-44da-84c0-3bfc3a7a3e62")] // IID_IDXGIFactory7
internal interface IDXGIFactory7 : IDXGIFactory6
{
    [PreserveSig]
    int RegisterAdaptersChangedEvent(IntPtr hEvent, out uint pdwCookie);

    [PreserveSig]
    int UnregisterAdaptersChangedEvent(uint dwCookie);
}

// The full IDXGIObject has 4 methods (slots #0..#3).
// Then IDXGIOutput adds its methods (slots #4..#..).
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("AE02EEDB-C735-4690-8D52-5A8DC20213AA")] // IID_IDXGIOutput
internal interface IDXGIOutput
{
    // -------- IDXGIObject (slots #0..#3) --------

    // 0) HRESULT SetPrivateData(REFGUID Name, UINT DataSize, const void *pData);
    [PreserveSig]
    int SetPrivateData(
        ref Guid Name,
        int DataSize,
        IntPtr pData
    );

    // 1) HRESULT SetPrivateDataInterface(REFGUID Name, const IUnknown *pUnknown);
    [PreserveSig]
    int SetPrivateDataInterface(
        ref Guid Name,
        IntPtr pUnknown
    );

    // 2) HRESULT GetPrivateData(REFGUID Name, UINT *pDataSize, void *pData);
    [PreserveSig]
    int GetPrivateData(
        ref Guid Name,
        ref int pDataSize,
        IntPtr pData
    );

    // 3) HRESULT GetParent(REFGUID riid, void **ppParent);
    [PreserveSig]
    int GetParent(
        ref Guid riid,
        out IntPtr ppParent
    );

    // -------- IDXGIOutput (slots #4..#16) --------

    // 4) HRESULT GetDesc(DXGI_OUTPUT_DESC *pDesc);
    [PreserveSig]
    int GetDesc(
        out DXGI_OUTPUT_DESC pDesc
    );

    // 5) HRESULT GetDisplayModeList(DXGI_FORMAT EnumFormat, UINT Flags,
    //       UINT *pNumModes, DXGI_MODE_DESC *pDesc);
    [PreserveSig]
    int GetDisplayModeList(
        int enumFormat, // treat DXGI_FORMAT as int or define your own enum
        uint flags,
        ref int pNumModes,
        IntPtr pDesc // or DXGI_MODE_DESC*
    );

    // 6) HRESULT FindClosestMatchingMode(const DXGI_MODE_DESC *pModeToMatch,
    //       DXGI_MODE_DESC *pClosestMatch, IUnknown *pConcernedDevice);
    [PreserveSig]
    int FindClosestMatchingMode(
        IntPtr pModeToMatch,
        IntPtr pClosestMatch,
        IntPtr pConcernedDevice
    );

    // 7) HRESULT WaitForVBlank();
    [PreserveSig]
    int WaitForVBlank();

    // 8) HRESULT TakeOwnership(IUnknown *pDevice, BOOL Exclusive);
    [PreserveSig]
    int TakeOwnership(
        IntPtr pDevice,
        [MarshalAs(UnmanagedType.Bool)] bool exclusive
    );

    // 9) void ReleaseOwnership();
    void ReleaseOwnership();

    // 10) HRESULT GetGammaControlCapabilities(DXGI_GAMMA_CONTROL_CAPABILITIES *pGammaCaps);
    [PreserveSig]
    int GetGammaControlCapabilities(IntPtr pGammaCaps);

    // 11) HRESULT SetGammaControl(const DXGI_GAMMA_CONTROL *pArray);
    [PreserveSig]
    int SetGammaControl(IntPtr pArray);

    // 12) HRESULT GetGammaControl(DXGI_GAMMA_CONTROL *pArray);
    [PreserveSig]
    int GetGammaControl(IntPtr pArray);

    // 13) HRESULT SetDisplaySurface(IDXGISurface *pScanoutSurface);
    [PreserveSig]
    int SetDisplaySurface(IntPtr pScanoutSurface);

    // 14) HRESULT GetDisplaySurfaceData(IDXGISurface *pDestination);
    [PreserveSig]
    int GetDisplaySurfaceData(IntPtr pDestination);

    // 15) HRESULT GetFrameStatistics(DXGI_FRAME_STATISTICS *pStats);
    [PreserveSig]
    int GetFrameStatistics(IntPtr pStats);
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("00CDDEA8-939B-4B83-A340-A685226666CC")] // IID_IDXGIOutput1
internal interface IDXGIOutput1 : IDXGIOutput
{
    // idxgioutput1 is basically idxgioutput plus some extra methods:
    // e.g. GetDisplayModeList1, FindClosestMatchingMode1, etc.
    // We'll just stub them in order, then define DuplicateOutput.

    // 16) HRESULT GetDisplayModeList1(...);
    [PreserveSig]
    int GetDisplayModeList1(
        int enumFormat,
        uint flags,
        ref int pNumModes,
        IntPtr pDesc
    );

    // 17) HRESULT FindClosestMatchingMode1(...);
    [PreserveSig]
    int FindClosestMatchingMode1(
        IntPtr pModeToMatch,
        IntPtr pClosestMatch,
        IntPtr pConcernedDevice
    );

    // 18) HRESULT GetDisplaySurfaceData1(...);
    [PreserveSig]
    int GetDisplaySurfaceData1(IntPtr pDestination);

    // 19) HRESULT DuplicateOutput( [in] IUnknown *pDevice, [out] IDXGIOutputDuplication **ppOutputDuplication );
    [PreserveSig]
    int DuplicateOutput(
        IntPtr pDevice, // ID3D11Device*
        out IntPtr ppOutputDuplication // IDXGIOutputDuplication**
    );
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("191cfac3-a341-470d-b26e-a864f428319c")]
internal interface IDXGIOutputDuplication
{
    [PreserveSig]
    int AcquireNextFrame(
        int TimeoutInMilliseconds,
        out DXGI_OUTDUPL_FRAME_INFO pFrameInfo,
        out IntPtr ppDesktopResource /* IDXGIResource** */
    );

    [PreserveSig]
    int GetFrameDirtyRects(
        uint DirtyRectsBufferSize,
        IntPtr pDirtyRectsBuffer,
        out uint pDirtyRectsBufferSizeRequired
    );

    [PreserveSig]
    int GetFrameMoveRects(
        uint MoveRectsBufferSize,
        IntPtr pMoveRectBuffer,
        out uint pMoveRectsBufferSizeRequired
    );

    [PreserveSig]
    int GetFramePointerShape(
        uint PointerShapeBufferSize,
        IntPtr pPointerShapeBuffer,
        out uint pPointerShapeBufferSizeRequired,
        out DXGI_OUTDUPL_POINTER_SHAPE_INFO pPointerShapeInfo
    );

    [PreserveSig]
    int MapDesktopSurface(out IntPtr pLockedRect);

    [PreserveSig]
    int UnMapDesktopSurface();

    [PreserveSig]
    int ReleaseFrame();
}

[StructLayout(LayoutKind.Sequential)]
internal struct DXGI_OUTDUPL_FRAME_INFO
{
    public long LastPresentTime;
    public long LastMouseUpdateTime;
    public uint AccumulatedFrames;
    [MarshalAs(UnmanagedType.Bool)] public bool RectsCoalesced;
    [MarshalAs(UnmanagedType.Bool)] public bool ProtectedContentMaskedOut;
    public DXGI_OUTDUPL_POINTER_POSITION PointerPosition;
    public uint TotalMetadataBufferSize;
    public uint PointerShapeBufferSize;
}

// This is the correct struct for pointer position in frame info:
[StructLayout(LayoutKind.Sequential)]
internal struct DXGI_OUTDUPL_POINTER_POSITION
{
    public POINT Position;
    [MarshalAs(UnmanagedType.Bool)] public bool Visible;
}

[StructLayout(LayoutKind.Sequential)]
public struct POINT
{
    public int X;
    public int Y;
}

// Shape info needed by GetFramePointerShape:
[StructLayout(LayoutKind.Sequential)]
internal struct DXGI_OUTDUPL_POINTER_SHAPE_INFO
{
    public uint Type;
    public uint Width;
    public uint Height;
    public uint Pitch;
    public POINT HotSpot;
}

// Minimal subset of D3D11_RESOURCE_DIMENSION (just for GetType):
internal enum D3D11_RESOURCE_DIMENSION
{
    UNKNOWN = 0,
    BUFFER = 1,
    TEXTURE1D = 2,
    TEXTURE2D = 3,
    TEXTURE3D = 4
}

// Minimal D3D11_TEXTURE2D_DESC struct
[StructLayout(LayoutKind.Sequential)]
internal struct D3D11_TEXTURE2D_DESC
{
    public uint Width;
    public uint Height;
    public uint MipLevels;
    public uint ArraySize;
    public int Format; // DXGI_FORMAT, treat as int or define your own enum
    public DXGI_SAMPLE_DESC SampleDesc;
    public int Usage; // D3D11_USAGE, treat as int or define your own enum
    public uint BindFlags;
    public uint CPUAccessFlags;
    public uint MiscFlags;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DXGI_SAMPLE_DESC
{
    public uint Count;
    public uint Quality;
}

// Minimal ID3D11Resource (just enough to get the vtable in correct order)
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("DC8E63F3-D12B-4952-B47B-5E45026A862D")]
internal interface ID3D11Resource
{
    // 0) void GetType(D3D11_RESOURCE_DIMENSION* pResourceDimension);
    void GetType(out D3D11_RESOURCE_DIMENSION resourceDimension);

    // 1) void SetEvictionPriority(UINT EvictionPriority);
    void SetEvictionPriority(uint EvictionPriority);

    // 2) UINT GetEvictionPriority();
    uint GetEvictionPriority();
}

// ID3D11Texture2D extends ID3D11Resource in the vtable
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("6f15aaf2-d208-4e89-9ab4-489535d34f9c")] // IID_ID3D11Texture2D
internal interface ID3D11Texture2D : ID3D11Resource
{
    // ID3D11Texture2D::GetDesc
    void GetDesc(out D3D11_TEXTURE2D_DESC desc);
}

// ------------------------------------------
// 1) Minimal struct for adapter description
// ------------------------------------------
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct DXGI_ADAPTER_DESC
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string Description;

    public uint VendorId;
    public uint DeviceId;
    public uint SubSysId;
    public uint Revision;
    public IntPtr DedicatedVideoMemory; // SIZE_T
    public IntPtr DedicatedSystemMemory; // SIZE_T
    public IntPtr SharedSystemMemory; // SIZE_T
    public LUID AdapterLuid;
}

[StructLayout(LayoutKind.Sequential)]
internal struct LUID
{
    public uint LowPart;
    public int HighPart;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct DXGI_OUTPUT_DESC
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string DeviceName;

    public RECT DesktopCoordinates;
    [MarshalAs(UnmanagedType.Bool)] public bool AttachedToDesktop;
    public DXGI_MODE_ROTATION Rotation;
    public IntPtr Monitor;
}

[StructLayout(LayoutKind.Sequential)]
internal struct RECT
{
    public int left;
    public int top;
    public int right;
    public int bottom;
}

// Matches DXGI_MODE_ROTATION_*
internal enum DXGI_MODE_ROTATION
{
    UNSPECIFIED = 0,
    IDENTITY = 1,
    ROTATE90 = 2,
    ROTATE180 = 3,
    ROTATE270 = 4
}

// ----------------------------------------------------------------
// D3D11 map/unmap structures & device context
// ----------------------------------------------------------------

internal enum D3D11_MAP
{
    READ = 1,
    WRITE = 2,
    READ_WRITE = 3,
    WRITE_DISCARD = 4,
    WRITE_NO_OVERWRITE = 5
}

[StructLayout(LayoutKind.Sequential)]
internal struct D3D11_MAPPED_SUBRESOURCE
{
    public IntPtr pData;
    public int RowPitch;
    public int DepthPitch;
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("C0BFA96C-E089-44FB-8EAF-26F8796190DA")] // IID_ID3D11DeviceContext
internal interface ID3D11DeviceContext
{
    // Many methods omitted. We only define the ones we actually call,
    // *in the correct order* they appear in the vtable:

    // 0  void VSSetConstantBuffers(...)
    void VSSetConstantBuffers();

    // 1  void PSSetShaderResources(...)
    void PSSetShaderResources();

    // 2  void PSSetShader(...)
    void PSSetShader();

    // 3  void PSSetSamplers(...)
    void PSSetSamplers();

    // 4  void VSSetShader(...)
    void VSSetShader();

    // 5  void DrawIndexed(...)
    void DrawIndexed();

    // 6  void Draw(...)
    void Draw();

    // 7  HRESULT Map(
    //       ID3D11Resource *pResource,
    //       UINT Subresource,
    //       D3D11_MAP MapType,
    //       UINT MapFlags,
    //       D3D11_MAPPED_SUBRESOURCE *pMappedResource);
    [PreserveSig]
    int Map(
        IntPtr pResource,
        uint subresource,
        D3D11_MAP mapType,
        uint mapFlags,
        out D3D11_MAPPED_SUBRESOURCE pMappedResource
    );

    // 8  void Unmap(ID3D11Resource *pResource, UINT Subresource);
    void Unmap(IntPtr pResource, uint subresource);

    // ... and so on
}

// ----------------------------------------------------------------
// Simple DesktopDuplicator class
// ----------------------------------------------------------------

public class DesktopDuplicator
{
    private IntPtr device = IntPtr.Zero; // ID3D11Device*
    private IntPtr deviceContext = IntPtr.Zero; // ID3D11DeviceContext*
    private IntPtr outputDuplication = IntPtr.Zero; // IDXGIOutputDuplication*

    private readonly int _x, _y, _width, _height;
    // keep the rest of your fields, device pointers, etc.

    private Action<string> _log;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    public DesktopDuplicator(int x, int y, int width, int height, Action<string> log)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        _log = log;

        //IntPtr dxgiModule = GetModuleHandle("dxgi.dll");
        //IntPtr procAddress = GetProcAddress(dxgiModule, "EnumAdapterByGpuPreference");

        //if (procAddress == IntPtr.Zero)
        //{
        //    throw new Exception("EnumAdapterByGpuPreference is not available in dxgi.dll");
        //}

        Initialize();
    }

    private void Initialize()
    {
        // 1) Create the Direct3D 11 device + device context
        int hr = D3D11.D3D11CreateDevice(
            IntPtr.Zero, // use default adapter
            D3D_DRIVER_TYPE.HARDWARE, // or WARP, etc.
            IntPtr.Zero,
            0,
            IntPtr.Zero, // no specific feature levels
            0,
            7, // D3D11_SDK_VERSION == 7
            out device,
            out _,
            out deviceContext
        );
        if (hr != 0)
            throw new Exception("D3D11CreateDevice failed: " + hr);

        // Attempt to create the highest available factory version
        // Attempt to create the highest available factory version
        IntPtr dxgiFactoryPtr = IntPtr.Zero;
        (int, Guid)[] factoryGuids =
        [
            (7, typeof(IDXGIFactory7).GUID),
            (6, typeof(IDXGIFactory6).GUID),
            (5, typeof(IDXGIFactory5).GUID),
            (4, typeof(IDXGIFactory4).GUID),
            (1, typeof(IDXGIFactory1).GUID)
        ];

        // Try to create DXGIFactory using the newest available version
        int highestSupportedFactory = 0;
        for (int i = 0; i < factoryGuids.Length; i++)
        {
            hr = D3D11.CreateDXGIFactory2(0, ref factoryGuids[i].Item2, out dxgiFactoryPtr);
            if (hr == 0)
            {
                highestSupportedFactory = factoryGuids[i].Item1;
                _log($"Successfully created factory version: IDXGIFactory{highestSupportedFactory}");
                break;
            }
        }

        // If CreateDXGIFactory2 completely fails, fallback to CreateDXGIFactory (DXGI 1.0)
        if (hr != 0)
        {
            hr = D3D11.CreateDXGIFactory(ref factoryGuids[^1].Item2, out dxgiFactoryPtr);
            if (hr != 0)
                throw new Exception($"Failed to create any DXGI factory: HRESULT = 0x{hr:X}");
        }

        if (dxgiFactoryPtr == IntPtr.Zero)
            throw new Exception("CreateDXGIFactory returned NULL pointer.");

        // Use the highest available version
        _log($"Using IDXGIFactory{highestSupportedFactory}.");
        IDXGIFactory factory = (IDXGIFactory)Marshal.GetObjectForIUnknown(dxgiFactoryPtr);

        IntPtr adapterPtr;
        // 4) Enumerate the first adapter from the factory
        if (false && highestSupportedFactory >= 6)
        {
            Guid factory6Guid = typeof(IDXGIFactory6).GUID;
            hr = Marshal.QueryInterface(dxgiFactoryPtr, ref factory6Guid, out IntPtr factory6Ptr);

            IDXGIFactory6 factory6;
            if (hr == 0 && factory6Ptr != IntPtr.Zero)
            {
                _log("Successfully queried IDXGIFactory6.");
                factory6 = (IDXGIFactory6)Marshal.GetObjectForIUnknown(factory6Ptr);
            }
            else
            {
                _log("Failed to query IDXGIFactory6, HRESULT = " + hr.ToString("X"));
                throw new Exception("Failed to query IDXGIFactory6.");
            }

            Guid adapterGuid = typeof(IDXGIAdapter).GUID;
            hr = factory6.EnumAdapterByGpuPreference(
                0U, // First adapter
                0U, // DXGI_GPU_PREFERENCE_UNSPECIFIED (or HIGH_PERFORMANCE for gaming GPUs)
                ref adapterGuid,
                out adapterPtr
            );
        }
        else if (highestSupportedFactory >= 1)
        {
            Guid factory1Guid = typeof(IDXGIFactory1).GUID;
            hr = Marshal.QueryInterface(dxgiFactoryPtr, ref factory1Guid, out IntPtr factory1Ptr);

            IDXGIFactory1 factory1;
            if (hr == 0 && factory1Ptr != IntPtr.Zero)
            {
                _log("Successfully queried IDXGIFactory1.");
                factory1 = (IDXGIFactory1)Marshal.GetObjectForIUnknown(factory1Ptr);
            }
            else
            {
                _log("Failed to query IDXGIFactory1, HRESULT = " + hr.ToString("X"));
                throw new Exception("Failed to query IDXGIFactory1.");
            }

            _log($"Using IDXGIFactory1 - falling back to EnumAdapters1. (Highest supported: {highestSupportedFactory})");

            _log($"Using IDXGIFactory1 - calling EnumAdapters1. (Highest supported: {highestSupportedFactory})");

            if (factory1 == null)
                throw new Exception("IDXGIFactory1 reference is NULL before EnumAdapters1.");

            if (dxgiFactoryPtr == IntPtr.Zero)
                throw new Exception("dxgiFactoryPtr is NULL before calling EnumAdapters1.");

            hr = factory1.EnumAdapters1(0, out adapterPtr);
        }
        else
        {
            _log("Using IDXGIFactory - falling back to EnumAdapters.");
            hr = factory.EnumAdapters(0, out adapterPtr);
        }

        if (hr != 0 || adapterPtr == IntPtr.Zero)
            throw new Exception("No IDXGIAdapter found for adapter: " + hr);

        // 5) Get the first output (e.g., the primary monitor)
        IDXGIAdapter adapter = (IDXGIAdapter)Marshal.GetObjectForIUnknown(adapterPtr);

        // We don�t have an explicit interface for IDXGIAdapter in this snippet,
        // so we can do the older approach: adapter->EnumOutputs(0, &output).
        // For brevity, let's do a direct QueryInterface from the factory
        // if you want the *first output.*
        // In a real app, you�d do adapter->EnumOutputs. For clarity, show a direct approach:
        Guid outputGuid = typeof(IDXGIOutput).GUID;
        hr = adapter.EnumOutputs(0, out IntPtr outputPtr);
        if (hr != 0 || outputPtr == IntPtr.Zero)
            throw new Exception("No IDXGIOutput found for adapter: " + hr);

        // 6) Query for IDXGIOutput1
        IDXGIOutput1 output1 = (IDXGIOutput1)Marshal.GetObjectForIUnknown(outputPtr);

        // 7) Duplicate the output via IDXGIOutput1::DuplicateOutput
        hr = output1.DuplicateOutput(device, out outputDuplication);
        if (hr != 0 || outputDuplication == IntPtr.Zero)
            throw new Exception("DuplicateOutput failed: " + hr);
    }

    public byte[] CaptureFrame()
    {
        if (outputDuplication == IntPtr.Zero)
            return null;

        var duplication = (IDXGIOutputDuplication)Marshal.GetObjectForIUnknown(outputDuplication);

        // Acquire next frame
        int hr = duplication.AcquireNextFrame(
            50, // 50ms timeout
            out DXGI_OUTDUPL_FRAME_INFO frameInfo,
            out IntPtr desktopResourcePtr
        );
        // If hr != 0, often it�s DXGI_ERROR_WAIT_TIMEOUT (0x887A0027)
        // which means no new frame is available:
        if (hr != 0 || desktopResourcePtr == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            // The resource is typically ID3D11Texture2D
            // Query for that
            Guid tex2dGuid = new Guid("6f15aaf2-d208-4e89-9ab4-489535d34f9c"); // IID_ID3D11Texture2D
            int qhr = Marshal.QueryInterface(desktopResourcePtr, ref tex2dGuid, out IntPtr texturePtr);
            if (qhr != 0 || texturePtr == IntPtr.Zero)
                return null;

            // Copy texture into CPU memory:
            byte[] data = CopyTextureToMemory(texturePtr);

            // Release the local texture pointer:
            Marshal.Release(texturePtr);

            return data;
        }
        finally
        {
            // Release the frame so the duplication can move on
            duplication.ReleaseFrame();
        }
    }

    private byte[] CopyTextureToMemory(IntPtr texturePtr)
    {
        if (deviceContext == IntPtr.Zero || texturePtr == IntPtr.Zero)
            return null;

        // 1) Query the ID3D11Texture2D interface
        var texture2D = (ID3D11Texture2D)Marshal.GetObjectForIUnknown(texturePtr);

        // 2) Get the texture’s description (actual full size, format, usage, etc.)
        texture2D.GetDesc(out D3D11_TEXTURE2D_DESC desc);
        int fullWidth = (int)desc.Width;
        int fullHeight = (int)desc.Height;

        // 3) Map the texture for CPU read access
        var ctx = (ID3D11DeviceContext)Marshal.GetObjectForIUnknown(deviceContext);
        int hr = ctx.Map(texturePtr, 0, D3D11_MAP.READ, 0, out D3D11_MAPPED_SUBRESOURCE mapped);
        if (hr != 0)
            return null;

        try
        {
            // -------------------------------------------------------------------
            // STEP A: Determine what region we can safely read within the texture
            // -------------------------------------------------------------------
            int x = _x;
            int y = _y;
            int regionWidth = _width;
            int regionHeight = _height;

            // Clamp to the actual texture boundaries
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x + regionWidth > fullWidth) regionWidth = fullWidth - x;
            if (y + regionHeight > fullHeight) regionHeight = fullHeight - y;

            // If the region is invalid, bail out
            if (regionWidth <= 0 || regionHeight <= 0)
                return null;

            // -------------------------------------------------------------------
            // STEP B: Copy the pixel data row by row into a managed byte array
            // -------------------------------------------------------------------
            int bytesPerPixel = 4; // Typically BGRA or RGBA in desktop duplication
            int outRowSize = regionWidth * bytesPerPixel;
            int totalBytes = outRowSize * regionHeight;
            byte[] frameData = new byte[totalBytes];

            // We'll do a row-by-row copy
            for (int row = 0; row < regionHeight; row++)
            {
                // Source pointer:
                // - offset by (y + row) rows in the mapped resource
                // - offset by x columns
                IntPtr src = mapped.pData
                             + ((y + row) * mapped.RowPitch)
                             + (x * bytesPerPixel);

                // Destination offset in our cropped buffer
                int dstOffset = row * outRowSize;

                // Copy one row
                Marshal.Copy(src, frameData, dstOffset, outRowSize);
            }

            return frameData;
        }
        finally
        {
            // 4) Unmap
            ctx.Unmap(texturePtr, 0);
        }
    }
}

// ------------------------------------------
// 2) IDXGIAdapter extends IDXGIObject
// ------------------------------------------
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("2411E7E1-12AC-4CCF-BD14-9798E8534DC0")] // IID_IDXGIAdapter
internal interface IDXGIAdapter
{
    // ---- IDXGIObject methods (slots #0..#3) ----

    // 0) HRESULT SetPrivateData(REFGUID Name, UINT DataSize, const void *pData);
    [PreserveSig]
    int SetPrivateData(
        ref Guid Name,
        int DataSize,
        IntPtr pData
    );

    // 1) HRESULT SetPrivateDataInterface(REFGUID Name, const IUnknown *pUnknown);
    [PreserveSig]
    int SetPrivateDataInterface(
        ref Guid Name,
        IntPtr pUnknown // IUnknown*
    );

    // 2) HRESULT GetPrivateData(REFGUID Name, UINT *pDataSize, void *pData);
    [PreserveSig]
    int GetPrivateData(
        ref Guid Name,
        ref int pDataSize,
        IntPtr pData
    );

    // 3) HRESULT GetParent(REFGUID riid, void **ppParent);
    [PreserveSig]
    int GetParent(
        ref Guid riid,
        out IntPtr ppParent
    );

    // ---- IDXGIAdapter methods (slots #4..#6) ----

    // 4) HRESULT EnumOutputs(UINT Output, IDXGIOutput **ppOutput);
    [PreserveSig]
    int EnumOutputs(
        uint outputIndex,
        out IntPtr ppOutput /* IDXGIOutput** */
    );

    // 5) HRESULT GetDesc(DXGI_ADAPTER_DESC *pDesc);
    [PreserveSig]
    int GetDesc(
        out DXGI_ADAPTER_DESC desc
    );

    // 6) HRESULT CheckInterfaceSupport(REFGUID InterfaceName, LARGE_INTEGER *pUMDVersion);
    [PreserveSig]
    int CheckInterfaceSupport(
        ref Guid InterfaceName,
        out long pUMDVersion
    );
}

public class WinStreamer : IStreamer, IDisposable
{
    private readonly ICaptureEventSource _eventSource;
    private DesktopDuplicator? _duplicator;
    private CancellationTokenSource? _cts;

    public event ExceptionHandler? OnException = delegate { };
    public event Action<string> OnLog = delegate { };

    public WinStreamer(ICaptureEventSource eventSource)
    {
        _eventSource = eventSource;
    }

    public ICaptureEventSource EventSource => _eventSource;

    public bool IsCapturing { get; private set; }

    public void Dispose()
    {
        Stop(); // ensure we clean up duplication & threads
        // If you have other resources, release them here
    }

    public Task<bool> CheckPermissionAsync()
    {
        // On Windows, usually no special permission needed for direct duplication.
        // Return true or handle any relevant checks you want.
        return Task.FromResult(true);
    }

    public void Start(int displayId, int x, int y, int width, int height, int frameRate)
    {
        // If already capturing, do nothing
        if (IsCapturing) return;

        // Instantiate DesktopDuplicator
        _duplicator = new DesktopDuplicator(x, y, width, height, OnLog);

        // Create a token source for the capture loop
        _cts = new CancellationTokenSource();
        IsCapturing = true;

        // Fire off a background task to capture frames continuously
        Task.Run(() => CaptureLoop(frameRate, _cts.Token));
    }

    private async Task CaptureLoop(int frameRate, CancellationToken token)
    {
        // For a simple example, just run as fast as possible
        // or do a small wait to target a specific frameRate
        int msPerFrame = frameRate > 0 ? (1000 / frameRate) : 33; // ~30fps fallback

        while (!token.IsCancellationRequested)
        {
            if (_duplicator == null)
                break;

            // Grab a frame
            try
            {
                var frameBytes = _duplicator.CaptureFrame();
                if (frameBytes == null)
                {
                    await Task.Delay(1000, token).ConfigureAwait(false);
                    continue;
                }

                _eventSource?.InvokeFrameCaptured(frameBytes);
            }
            catch (Exception ex)
            {
                OnException?.Invoke(ex);
            }

            // Respect the requested frame rate
            await Task.Delay(msPerFrame, token).ConfigureAwait(false);
        }
    }

    public void Stop()
    {
        if (!IsCapturing) return;
        IsCapturing = false;

        // Cancel the capture loop
        _cts?.Cancel();
        _cts = null;

        // No direct "cleanup" method in the snippet, but you could release duplication, etc.
        // If you want to force release or re-init, do:
        _duplicator = null;
    }
}