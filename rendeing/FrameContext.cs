using graphics;
using System.Drawing;
using Vortice.Mathematics;

namespace rendering;

public readonly record struct FrameContext(
    int BackBufferIndex,
    RenderTargetView RenderTargetView,
    DepthStencilView DepthStencilView,
    Viewport Viewport,
    Rectangle ScissorRect
);