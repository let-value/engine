using graphics;
using rendering.components;
using System.Drawing;
using Vortice.Mathematics;

namespace rendering;

public class NoopRenderPipeline : IRenderPipeline {
    public CommandList[] Render(int backBufferIndex,
        RenderTargetView renderTargetView,
        DepthStencilView depthStencilView,
        in ReadOnlySpan<IRenderable> renderables,
        Viewport viewport,
        Rectangle scissorRect
    ) =>
        Array.Empty<CommandList>();

    public void Dispose() {
        // TODO release managed resources here
    }
}