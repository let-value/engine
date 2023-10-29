using graphics;

namespace rendering;

public class NoopRenderPipeline : IRenderPipeline {
    public CommandListRequest GetCommandListCount() {
        return new(0);
    }

    public void Render(FrameContext frameContext) {
        // Noop
    }

    public void Dispose() {
        // TODO release managed resources here
    }
}