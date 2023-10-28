using graphics;

namespace rendering;

public interface IRenderPipeline : IDisposable {
    public CommandList[] Render(FrameContext frameContext);
}