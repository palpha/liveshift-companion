namespace Core.Image;

using System;

public class WinStreamer(ICaptureEventSource eventSource) : IStreamer, IDisposable
{
    public ICaptureEventSource EventSource { get; } = eventSource;

    public bool IsCapturing { get; private set; }

    public Task<bool> CheckPermissionAsync()
    {
        // On Windows, usually no special permission needed for direct duplication.
        return Task.FromResult(true);
    }

    public void Start(int displayId, int x, int y, int width, int height, int frameRate)
    {
        if (IsCapturing)
        {
            return;
        }


        IsCapturing = true;
    }

    public void Stop()
    {
        if (IsCapturing == false)
        {
            return;
        }

        IsCapturing = false;
    }

    public void Dispose()
    {
        Stop();
    }
}