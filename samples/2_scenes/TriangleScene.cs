using rendering;

namespace sample;

public class TriangleScene(TriangleRenderGraph triangleRenderGraph) : IScene {
    public IRenderGraph? RenderGraph { get; set; } = triangleRenderGraph;

    public void Dispose() {
        RenderGraph?.Dispose();
    }
}