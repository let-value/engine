using System.Collections.ObjectModel;
using System.Diagnostics;
using rendering;

namespace scene;

public class ScenesRenderGraph : IRenderGraph {
    private readonly SceneManager SceneManager;
    private ReadOnlyCollection<IScene> ForegroundScenes = null!;
    private readonly CommandListRequest CommandListRequest;

    public ScenesRenderGraph(SceneManager sceneManager) {
        SceneManager = sceneManager;
        CommandListRequest = new(1);

        sceneManager.ForegroundChanged += OnForegroundChanged;
        OnForegroundChanged();
    }

    private void OnForegroundChanged(object? sender = null, EventArgs? e = null) {
        ForegroundScenes = SceneManager.ForegroundScenes
            .Where(x => x.RenderGraph != null)
            .ToList()
            .AsReadOnly();

        CommandListRequest.ChildRequests.Value = ForegroundScenes
            .Select(x => x.RenderGraph!.GetCommandListCount())
            .ToArray();
    }

    public CommandListRequest GetCommandListCount() {
        return CommandListRequest;
    }

    public void Render(FrameContext frameContext) {
        for (var i = 0; i < ForegroundScenes.Count; i++) {
            var commandLists = CommandListRequest.Slice(frameContext.CommandLists, 0, i);
            var scene = ForegroundScenes[i];

            scene.RenderGraph?.Render(frameContext with { CommandLists = commandLists });
        }
    }

    public void Dispose() {
        SceneManager.ForegroundChanged -= OnForegroundChanged;
        CommandListRequest.Dispose();
    }
}