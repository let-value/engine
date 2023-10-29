namespace rendering;

public interface IRenderPipeline : IDisposable {
    CommandListRequest GetCommandListCount();
    void Render(FrameContext frameContext);
}