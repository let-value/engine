using System.Collections.ObjectModel;
using rendering;

namespace scene;

public class ScenesRenderPipeline : IRenderPipeline {
    private readonly SceneManager SceneManager;
    private ReadOnlyCollection<IScene> ForegroundScenes;
    private CommandListRequest CommandListRequest;

    public ScenesRenderPipeline(SceneManager sceneManager) {
        SceneManager = sceneManager;
        sceneManager.ForegroundChanged += OnForegroundChanged;
    }

    private void OnForegroundChanged(object sender, EventArgs e) {
        ForegroundScenes = SceneManager.ForegroundScenes.Where(x => x.RenderPipeline != null).ToList().AsReadOnly();
        CommandListRequest = new(1, ForegroundScenes.Select(x => x.RenderPipeline?.GetCommandListCount()).ToArray()!);
    }

    public CommandListRequest GetCommandListCount() {
        return CommandListRequest;
    }

    public void Render(FrameContext frameContext) {
        for (var i = 0; i < ForegroundScenes.Count; i++) {
            var commandLists = CommandListRequest.SliceChild(i, frameContext.CommandLists);
            var scene = ForegroundScenes[i];
            if (scene.RenderPipeline == null) {
                throw new InvalidOperationException("Scene has no render pipeline");
            }

            scene.RenderPipeline.Render(frameContext with { CommandLists = commandLists });
        }
    }

    public void Dispose() {
        SceneManager.ForegroundChanged -= OnForegroundChanged;
    }
}