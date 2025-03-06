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
        IntPtr pFeatureLevels,   // for simplicity, pass IntPtr.Zero
        uint FeatureLevels,      // count of levels in pFeatureLevels
        uint SDKVersion,
        out IntPtr ppDevice,     // ID3D11Device**
        out IntPtr pFeatureLevel,// D3D_FEATURE_LEVEL*
        out IntPtr ppImmediateContext // ID3D11DeviceContext**
    );

    [DllImport(DXGI_DLL, CallingConvention = CallingConvention.StdCall)]
    public static extern int CreateDXGIFactory(ref Guid riid, out IntPtr ppFactory);
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

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("7B7166EC-21C7-44AE-B21A-C9AE321AE369")] // IID_IDXGIFactory
internal interface IDXGIFactory
{
    // 0) HRESULT SetPrivateData(...)
    [PreserveSig]
    int SetPrivateData(
        ref Guid Name,
        int DataSize,
        IntPtr pData
    );

    // 1) HRESULT SetPrivateDataInterface(...)
    [PreserveSig]
    int SetPrivateDataInterface(
        ref Guid Name,
        IntPtr pUnknown // IUnknown*
    );

    // 2) HRESULT GetPrivateData(...)
    [PreserveSig]
    int GetPrivateData(
        ref Guid Name,
        ref int pDataSize,
        IntPtr pData
    );

    // 3) HRESULT GetParent(...)
    [PreserveSig]
    int GetParent(
        ref Guid riid,
        out IntPtr ppParent
    );

    // 4) HRESULT EnumAdapters(UINT Adapter, IDXGIAdapter** ppAdapter);
    [PreserveSig]
    int EnumAdapters(
        uint adapterIndex,
        out IntPtr ppAdapter
    );

    // For completeness, you might add the rest (MakeWindowAssociation, etc.)
    // but at least this ensures vtable alignment up to EnumAdapters.
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
        int enumFormat,   // treat DXGI_FORMAT as int or define your own enum
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
        IntPtr pDevice,                         // ID3D11Device*
        out IntPtr ppOutputDuplication          // IDXGIOutputDuplication**
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
    [MarshalAs(UnmanagedType.Bool)]
    public bool RectsCoalesced;
    [MarshalAs(UnmanagedType.Bool)]
    public bool ProtectedContentMaskedOut;
    public DXGI_OUTDUPL_POINTER_POSITION PointerPosition;
    public uint TotalMetadataBufferSize;
    public uint PointerShapeBufferSize;
}

// This is the correct struct for pointer position in frame info:
[StructLayout(LayoutKind.Sequential)]
internal struct DXGI_OUTDUPL_POINTER_POSITION
{
    public POINT Position;
    [MarshalAs(UnmanagedType.Bool)]
    public bool Visible;
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
    public int Usage;  // D3D11_USAGE, treat as int or define your own enum
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
    public IntPtr DedicatedVideoMemory;   // SIZE_T
    public IntPtr DedicatedSystemMemory;  // SIZE_T
    public IntPtr SharedSystemMemory;     // SIZE_T
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
    [MarshalAs(UnmanagedType.Bool)]
    public bool AttachedToDesktop;
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
    private IntPtr device = IntPtr.Zero;         // ID3D11Device*
    private IntPtr deviceContext = IntPtr.Zero;  // ID3D11DeviceContext*
    private IntPtr outputDuplication = IntPtr.Zero; // IDXGIOutputDuplication*

    private readonly int _x, _y, _width, _height;
    // keep the rest of your fields, device pointers, etc.

    public DesktopDuplicator(int x, int y, int width, int height)
    {
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        Initialize();
    }

    private void Initialize()
    {
        // 1) Create the Direct3D 11 device + device context
        int hr = D3D11.D3D11CreateDevice(
            IntPtr.Zero,                // use default adapter
            D3D_DRIVER_TYPE.HARDWARE,   // or WARP, etc.
            IntPtr.Zero,
            0,
            IntPtr.Zero,                // no specific feature levels
            0,
            7, // D3D11_SDK_VERSION == 7
            out device,
            out _,
            out deviceContext
        );
        if (hr != 0)
            throw new Exception("D3D11CreateDevice failed: " + hr);

        // 2) Create a DXGI Factory
        Guid factoryGuid = typeof(IDXGIFactory).GUID; // or new Guid("7B7166EC-21C7-44AE-B21A-C9AE321AE369")
        hr = D3D11.CreateDXGIFactory(ref factoryGuid, out IntPtr dxgiFactoryPtr);
        if (hr != 0)
            throw new Exception("CreateDXGIFactory failed: " + hr);

        // 3) Get IDXGIFactory from the pointer
        IDXGIFactory factory = (IDXGIFactory)Marshal.GetObjectForIUnknown(dxgiFactoryPtr);

        // 4) Enumerate the first adapter from the factory
        hr = factory.EnumAdapters(0, out IntPtr adapterPtr); // get first adapter
        if (hr != 0 || adapterPtr == IntPtr.Zero)
            throw new Exception("EnumAdapters failed or no adapter found: " + hr);

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
        _duplicator = new DesktopDuplicator(x, y, width, height);

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
            byte[]? frameBytes = _duplicator.CaptureFrame();
            if (frameBytes != null)
            {
                // TODO: Do something with the captured data
                // e.g., pass it to your event source or a callback
                // _eventSource?.OnFrameCaptured(frameBytes);

                // Or, if you have some delegate/event on WinStreamer itself, raise it
                // OnFrameCaptured?.Invoke(this, frameBytes);
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