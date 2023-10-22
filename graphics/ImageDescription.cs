using Vortice.Direct3D12;
using Vortice.DXGI;

namespace graphics;

public record struct ImageDescription {
    public short DepthOrArraySize { get; init; }
    public ResourceDimension Dimension { get; init; }
    public Format Format { get; init; }
    public int Height { get; init; }
    public short MipLevels { get; init; }
    public int Width { get; init; }

    public static ImageDescription New2D(int width, int height, Format format, short mipCount = 1,
        short arraySize = 1) {
        return new() {
            Dimension = ResourceDimension.Texture2D,
            Width = width,
            Height = height,
            DepthOrArraySize = arraySize,
            Format = format,
            MipLevels = mipCount
        };
    }
}