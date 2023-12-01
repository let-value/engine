using rendering;
using System.Collections.ObjectModel;

namespace routing;

public class RoutingRenderGraph : IRenderGraph {
    private readonly RouteManager RouteManager;
    private ReadOnlyCollection<IRoute> ForegroundScenes = null!;
    private readonly CommandListRequest CommandListRequest;

    public RoutingRenderGraph(RouteManager routeManager) {
        RouteManager = routeManager;
        CommandListRequest = new(1);

        routeManager.ForegroundChanged += OnForegroundChanged;
        OnForegroundChanged();
    }

    private void OnForegroundChanged(object? sender = null, EventArgs? e = null) {
        ForegroundScenes = RouteManager.ForegroundRoutes
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
        RouteManager.ForegroundChanged -= OnForegroundChanged;
        CommandListRequest.Dispose();
    }
}