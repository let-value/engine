using System.Drawing;
using graphics;
using rendering.components;
using Vortice.Mathematics;

namespace rendering;

public interface IRenderPipeline : IDisposable {
    public CommandList[] Render(int backBufferIndex,
        RenderTargetView renderTargetView,
        DepthStencilView depthStencilView,
        in ReadOnlySpan<IRenderable> renderables,
        Viewport viewport,
        Rectangle scissorRect
    );
}