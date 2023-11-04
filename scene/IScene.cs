using rendering;

namespace scene;

public interface IScene : IDisposable {
    public bool IsModal => false;
    public IRenderGraph? RenderGraph => default;
}