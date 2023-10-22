namespace rendering;

public interface IRenderer : IDisposable {
    void OnLoad();
    void OnRender();
}