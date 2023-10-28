using graphics;

namespace rendering;

public class NoopRenderPipeline : IRenderPipeline {
    public CommandList[] Render(FrameContext context) => Array.Empty<CommandList>();

    public void Dispose() {
        // TODO release managed resources here
    }
}