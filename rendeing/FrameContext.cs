using graphics;
using System.Drawing;
using Vortice.Mathematics;

namespace rendering;

public ref struct FrameContext(
    int backBufferIndex,
    double deltaTime,
    ReadOnlySpan<CommandList> commandLists,
    RenderTargetView renderTargetView,
    DepthStencilView depthStencilView,
    Viewport viewport,
    Rectangle scissorRect
) {
    public int BackBufferIndex = backBufferIndex;
    public double DeltaTime = deltaTime;
    public ReadOnlySpan<CommandList> CommandLists = commandLists;
    public RenderTargetView RenderTargetView = renderTargetView;
    public DepthStencilView DepthStencilView = depthStencilView;
    public Viewport Viewport = viewport;
    public Rectangle ScissorRect = scissorRect;

    public void Deconstruct(
        out int backBufferIndex,
        out double deltaTime,
        out ReadOnlySpan<CommandList> commandLists,
        out RenderTargetView renderTargetView,
        out DepthStencilView depthStencilView,
        out Viewport viewport,
        out Rectangle scissorRect
    ) {
        backBufferIndex = BackBufferIndex;
        deltaTime = DeltaTime;
        commandLists = CommandLists;
        renderTargetView = RenderTargetView;
        depthStencilView = DepthStencilView;
        viewport = Viewport;
        scissorRect = ScissorRect;
    }
}