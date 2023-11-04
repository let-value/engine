using graphics;

namespace rendering;

public class NoopRenderGraph : IRenderGraph {
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