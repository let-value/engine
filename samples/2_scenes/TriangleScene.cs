using rendering;
using scene;

namespace sample;

public class TriangleScene(TriangleRenderingPipeline triangleRenderingPipeline) : IScene {
    public IRenderPipeline? RenderPipeline { get; set; } = triangleRenderingPipeline;

    public void Dispose() {
        RenderPipeline?.Dispose();
    }
}