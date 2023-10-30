using System.Collections.ObjectModel;
using System.Diagnostics;
using rendering;

namespace scene;

public class ScenesRenderPipeline : IRenderPipeline {
    private readonly SceneManager SceneManager;
    private ReadOnlyCollection<IScene> ForegroundScenes = null!;
    private CommandListRequest CommandListRequest = null!;

    public ScenesRenderPipeline(SceneManager sceneManager) {
        SceneManager = sceneManager;
        sceneManager.ForegroundChanged += OnForegroundChanged;
        OnForegroundChanged();
    }

    private void OnForegroundChanged(object? sender = null, EventArgs? e = null) {
        ForegroundScenes = SceneManager.ForegroundScenes
            .Where(x => x.RenderPipeline != null)
            .ToList()
            .AsReadOnly();
        CommandListRequest = new(1, ForegroundScenes.Select(x => x.RenderPipeline?.GetCommandListCount()).ToArray()!);
    }

    public CommandListRequest GetCommandListCount() {
        return CommandListRequest;
    }

    public void Render(FrameContext frameContext) {
        for (var i = 0; i < ForegroundScenes.Count; i++) {
            var commandLists = CommandListRequest.Slice(frameContext.CommandLists, 0, i);
            var scene = ForegroundScenes[i];

            scene.RenderPipeline?.Render(frameContext with { CommandLists = commandLists });
        }
    }

    public void Dispose() {
        SceneManager.ForegroundChanged -= OnForegroundChanged;
    }
}