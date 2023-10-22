using Vortice.DXGI;

namespace rendering;

public record struct PresentationParameters() {
    public int BackBufferWidth { init; get; } = default;
    public int BackBufferHeight { init; get; } = default;
    public Format BackBufferFormat { init; get; } = Format.B8G8R8A8_UNorm;
    public Format DepthStencilFormat { init; get; } = Format.D32_Float;
    public PresentParameters PresentParameters { init; get; } = default;
    public bool Stereo { init; get; } = default;
    public int SyncInterval { init; get; } = 1;
}