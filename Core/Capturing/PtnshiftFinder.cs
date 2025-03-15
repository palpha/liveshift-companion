using System.Diagnostics.CodeAnalysis;

namespace Core.Capturing;

public interface IPtnshiftFinder
{
    public record Location(int X, int Y);

    event Action LocationLost;
    event Action<Location> LocationFound;
    void OnFullScreenCapture(int width, ReadOnlySpan<byte> buffer);
    void OnRegionCapture(int posX, int posY, ReadOnlySpan<byte> buffer);
}

public class PtnshiftFinder : IPtnshiftFinder
{
    // abaabbbaaaabbbbb, where a = 1c1c1cff and b = 2c2c2cff

    private static readonly byte[] PixelA = [0x1C, 0x1C, 0x1C, 0xFF];
    private static readonly byte[] PixelB = [0x2C, 0x2C, 0x2C, 0xFF];

    private static readonly byte[] ExpectedBytes = new[]
        {
            PixelA, PixelB, PixelA, PixelA,
            PixelB, PixelB, PixelB, PixelA,
            PixelA, PixelA, PixelA, PixelB,
            PixelB, PixelB, PixelB, PixelB
        }
        .SelectMany(x => x).ToArray();

    private bool IsLocationLost { get; set; }
    private int RegionFrameCount { get; set; }
    private byte[]? LastFullScreenBuffer { get; set; }
    private int LastFullScreenWidth { get; set; }

    public event Action LocationLost = delegate { };
    public event Action<IPtnshiftFinder.Location> LocationFound = delegate { };

    public void OnFullScreenCapture(int width, ReadOnlySpan<byte> buffer)
    {
        LastFullScreenBuffer = buffer.ToArray();
        LastFullScreenWidth = width;
    }

    private void SetLocationLost()
    {
        if (IsLocationLost)
        {
            return;
        }

        IsLocationLost = true;
        LocationLost.Invoke();
    }

    private void SetLocationFound(IPtnshiftFinder.Location location)
    {
        IsLocationLost = false;
        LocationFound.Invoke(location);
    }

    public void OnRegionCapture(int posX, int posY, ReadOnlySpan<byte> buffer)
    {
        if (RegionFrameCount++ % 10 != 0)
        {
            // Only check every 10th frame
            return;
        }

        if (FindInBuffer(buffer, 960, posX, posY, out var location))
        {
            if (location.X == posX && location.Y == posY)
            {
                IsLocationLost = false;
                return;
            }

            SetLocationLost();
            SetLocationFound(location);

            return;
        }

        SetLocationLost();

        if (LastFullScreenBuffer == null)
        {
            return;
        }

        _ = FindInFullScreenAsync();
    }

    private async Task FindInFullScreenAsync()
    {
        if (LastFullScreenBuffer == null)
        {
            return;
        }

        try
        {
            await Task.CompletedTask;
            if (FindInBuffer(
                LastFullScreenBuffer,
                LastFullScreenWidth,
                posX: 0, posY: 0,
                out var location))
            {
                SetLocationFound(location);
            }
        }
        catch
        {
            //
        }
    }

    private static bool FindInBuffer(
        ReadOnlySpan<byte> buffer,
        int width, int posX, int posY,
        [NotNullWhen(true)] out IPtnshiftFinder.Location? location)
    {
        var index = buffer.IndexOf(ExpectedBytes);

        if (index == -1)
        {
            // Left frame
            location = null;
            return false;
        }

        var pixelIndex = index / 4;
        var x = posX + pixelIndex % width;
        var y = posY + pixelIndex / width;
        location = new(x, y);
        return true;
    }
}