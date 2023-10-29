using graphics;
using System.Drawing;
using Vortice.Mathematics;

namespace rendering;

public ref struct FrameContext {
    public int BackBufferIndex;
    public ReadOnlySpan<CommandList> CommandLists;
    public RenderTargetView RenderTargetView;
    public DepthStencilView DepthStencilView;
    public Viewport Viewport;
    public Rectangle ScissorRect;

    public FrameContext(
        int BackBufferIndex,
        ReadOnlySpan<CommandList> CommandLists,
        RenderTargetView RenderTargetView,
        DepthStencilView DepthStencilView,
        Viewport Viewport,
        Rectangle ScissorRect
    ) {
        this.BackBufferIndex = BackBufferIndex;
        this.CommandLists = CommandLists;
        this.RenderTargetView = RenderTargetView;
        this.DepthStencilView = DepthStencilView;
        this.Viewport = Viewport;
        this.ScissorRect = ScissorRect;
    }

    public void Deconstruct(
        out int backBufferIndex,
        out ReadOnlySpan<CommandList> commandLists,
        out RenderTargetView renderTargetView,
        out DepthStencilView depthStencilView,
        out Viewport viewport,
        out Rectangle scissorRect
    ) {
        backBufferIndex = BackBufferIndex;
        commandLists = CommandLists;
        renderTargetView = RenderTargetView;
        depthStencilView = DepthStencilView;
        viewport = Viewport;
        scissorRect = ScissorRect;
    }
}