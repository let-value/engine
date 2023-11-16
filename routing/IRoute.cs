using rendering;

namespace routing;

public interface IRoute : IDisposable {
    public bool IsModal => false;
    public IRenderGraph? RenderGraph => default;
}