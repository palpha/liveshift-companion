using Microsoft.Extensions.Logging;

namespace Core.Capturing;

public abstract class CaptureServiceBase(
    IStreamer streamer,
    IDisplayService displayService,
    ILogger<CaptureServiceBase> logger)
    : ICaptureService
{
    private int displayWidth;
    private int displayHeight;
    
    protected IStreamer Streamer { get; } = streamer;

    private IDisplayService DisplayService { get; } = displayService;

    protected ILogger<CaptureServiceBase> Logger { get; } = logger;
    protected CaptureConfiguration? CurrentConfiguration { get; set; }

    public event Action<ReadOnlySpan<byte>>? FrameCaptured;

    public bool IsCapturing => Streamer.IsCapturing;

    public abstract Task<bool> CheckCapturePermissionAsync();

    public virtual void SetConfiguration(CaptureConfiguration configuration)
    {
        var previousConfiguration = CurrentConfiguration;
        CurrentConfiguration = configuration;

        var display = DisplayService.GetDisplay(CurrentConfiguration!.DisplayId);
        displayHeight = display?.Height ?? 0;
        displayWidth = display?.Width ?? 0;

        if (previousConfiguration != null)
        {
            UpdateStreamerConfiguration(previousConfiguration);
        }
    }

    public virtual void StartCapture()
    {
        if (CurrentConfiguration == null)
        {
            throw new InvalidOperationException("Configuration not set.");
        }

        if (IsCapturing || CurrentConfiguration.IsValid(DisplayService.AvailableDisplays) == false)
        {
            return;
        }

        Streamer.EventSource.RegionFrameCaptured += OnRegionFrameReceived;
        Streamer.EventSource.FullScreenFrameCaptured += OnFullScreenFrameReceived;
        StartStreamer();
    }

    public void StopCapture()
    {
        if (IsCapturing == false)
        {
            return;
        }

        Streamer.EventSource.RegionFrameCaptured -= OnRegionFrameReceived;
        Streamer.EventSource.FullScreenFrameCaptured -= OnFullScreenFrameReceived;
        Streamer.Stop();
    }

    protected void StartStreamer()
    {
        if (CurrentConfiguration == null || CurrentConfiguration.DisplayId.HasValue == false)
        {
            throw new InvalidOperationException("Configuration/display not set.");
        }

        try
        {
            Streamer.Start(
                CurrentConfiguration.DisplayId.Value,
                CurrentConfiguration.CaptureX,
                CurrentConfiguration.CaptureY,
                CurrentConfiguration.Width,
                CurrentConfiguration.Height,
                CurrentConfiguration.FrameRate);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unable to start capture");
            throw;
        }
    }

    private void OnRegionFrameReceived(ReadOnlySpan<byte> frame) =>
        FrameCaptured?.Invoke(frame);

    private byte[] lastFullScreenFrame = [];
    private int lastFullScreenFrameWidth;
    private int lastFullScreenFrameHeight;

    private void OnFullScreenFrameReceived(ReadOnlySpan<byte> frame)
    {
        lastFullScreenFrame = frame.ToArray();
        lastFullScreenFrameWidth = displayWidth;
        lastFullScreenFrameHeight = displayHeight;
    }

    private const string TargetPattern = "abaabbbaaaabbbbb";

    private char[] MapFrameToCodes(byte[] fullFrame, int width, int height)
    {
        // One row has width * 4 bytes (assuming RGBA)
        // We'll create a big char[] with the same number of pixels:
        var mapped = new char[width * height];

        for (var i = 0; i < height; i++)
        {
            for (var j = 0; j < width; j++)
            {
                // Index of the pixel in the byte array
                var pixelIndex = (i * width + j) * 4;

                // Extract color (assuming RGBA)
                var r = fullFrame[pixelIndex + 0];
                var g = fullFrame[pixelIndex + 1];
                var b = fullFrame[pixelIndex + 2];
                // alpha = fullFrame[pixelIndex + 3];

                // Compare with #1C1C1C or #2C2C2C
                if (r == 0x1C && g == 0x1C && b == 0x1C)
                {
                    mapped[i * width + j] = 'a';
                }
                else if (r == 0x2C && g == 0x2C && b == 0x2C)
                {
                    mapped[i * width + j] = 'b';
                }
                else
                {
                    mapped[i * width + j] = 'x';
                }
            }
        }

        return mapped;
    }

    public void LocatePatternInFullScreen()
    {
        if (lastFullScreenFrame.Length == 0)
        {
            return;
        }

        var mapped = MapFrameToCodes(lastFullScreenFrame, lastFullScreenFrameWidth, lastFullScreenFrameHeight);

        for (var row = 0; row < lastFullScreenFrameHeight; row++)
        {
            var rowStart = row * lastFullScreenFrameWidth;
            var rowSpan = mapped.AsSpan(rowStart, lastFullScreenFrameWidth);
            var rowString = new string(rowSpan);
            var index = rowString.IndexOf(TargetPattern, StringComparison.Ordinal);

            if (rowString.IndexOf("a") > -1)
            {
            }
            
            if (index == -1)
            {
                continue;
            }

            OnPatternFound(index, row);
            return;
        }
    }

    public event Action<int, int>? PatternFound;

    private void OnPatternFound(int x, int y)
    {
        PatternFound?.Invoke(x, y);
    }

    protected abstract void UpdateStreamerConfiguration(CaptureConfiguration previousConfiguration);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        StopCapture();
    }
}