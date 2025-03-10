namespace Core.Image;

public delegate void FrameCapturedHandler(ReadOnlySpan<byte> frameBytes);

public interface ICaptureEventSource
{
    event FrameCapturedHandler FrameCaptured;
    void InvokeFrameCaptured(ReadOnlySpan<byte> frameBytes);
}

public interface IStreamer
{
    ICaptureEventSource EventSource { get; }
    Task<bool> CheckPermissionAsync();
    bool IsCapturing { get; }
    void Start(int displayId, int x, int y, int width, int height, int frameRate);
    void Stop();
}