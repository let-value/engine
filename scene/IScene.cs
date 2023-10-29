using rendering;

namespace scene;

public interface IScene : IDisposable {
    public bool IsModal => false;
    public IRenderPipeline? RenderPipeline => null;
}