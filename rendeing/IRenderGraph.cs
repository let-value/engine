namespace rendering;

public interface IRenderGraph : IDisposable {
    CommandListRequest GetCommandListCount();
    void Render(FrameContext frameContext);
}