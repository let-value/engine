using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;

namespace routing;

record FlatRoute(string Path, RouteDefinition Definition);

public record StackRoute(string Path, IRoute Instance, IServiceScope Scope);

public delegate void ForegroundChangedHandler(object sender, EventArgs e);

public class RouteManager {
    private readonly IServiceProvider Services;
    private readonly List<FlatRoute> Routes;

    public string Location = "";
    public readonly Stack<StackRoute> RouteStack = new();

    public ReadOnlyCollection<IRoute> ForegroundRoutes = new(Array.Empty<IRoute>());
    public event ForegroundChangedHandler? ForegroundChanged;

    public RouteManager(IOptions<RouteOptions> scenesOptions, IServiceProvider services) {
        Services = services;
        Routes = FlattenRouteDictionary(scenesOptions.Value.Routes);

        NavigateTo(scenesOptions.Value.InitialRoute);
    }

    public void NavigateTo(string path) {
        var newLocation = ToUnixSeparator(Path.Combine(ToSystemSeparator(Location), ToSystemSeparator(path)));

        UpdateRouteStack(newLocation);

        Location = newLocation;
    }

    private void UpdateRouteStack(string newLocation) {
        var toDispose = new List<StackRoute>();
        while (RouteStack.Count > 0 && !newLocation.StartsWith(RouteStack.Peek().Path)) {
            var scene = RouteStack.Pop();
            toDispose.Add(scene);
        }

        foreach (var scene in Routes) {
            if (!newLocation.StartsWith(scene.Path)) {
                continue;
            }

            if (RouteStack.Count > 0 && RouteStack.Peek().Path == scene.Path) {
                continue;
            }

            var scope = Services.CreateScope();

            if (scope.ServiceProvider.GetRequiredService(scene.Definition.Route) is not IRoute instance) {
                throw new InvalidOperationException($"Route {scene.Path} is not an IRoute");
            }

            RouteStack.Push(new(scene.Path, instance, scope));
        }

        if (RouteStack.Count == 0 || RouteStack.Peek().Path != newLocation) {
            throw new InvalidOperationException($"Route {newLocation} not found");
        }

        UpdateForegroundRoutes();

        foreach (var scene in toDispose) {
            scene.Instance.Dispose();
            scene.Scope.Dispose();
        }
    }

    private void UpdateForegroundRoutes() {
        var foregroundScenes = new List<IRoute>();

        foreach (var stackRoute in RouteStack.Reverse()) {
            foregroundScenes.Add(stackRoute.Instance);

            if (!stackRoute.Instance.IsModal) {
                break;
            }
        }

        ForegroundRoutes = new(foregroundScenes);
        ForegroundChanged?.Invoke(this, EventArgs.Empty);
    }

    private static List<FlatRoute> FlattenRouteDictionary(
        Dictionary<string, RouteDefinition> routes,
        string? parentPath = null
    ) {
        var result = new List<FlatRoute>();

        foreach (var kvp in routes) {
            var combinedPath = ToUnixSeparator(
                string.IsNullOrEmpty(parentPath)
                    ? kvp.Key
                    : Path.Combine(
                        ToSystemSeparator(parentPath),
                        ToSystemSeparator(kvp.Key)
                    )
            );

            result.Add(new(combinedPath, kvp.Value));

            if (kvp.Value.Routes != null) {
                result.AddRange(FlattenRouteDictionary(kvp.Value.Routes, combinedPath));
            }
        }

        return result;
    }

    private static string ToSystemSeparator(string path) => path
        .Replace('/', Path.DirectorySeparatorChar)
        .Replace('\\', Path.DirectorySeparatorChar);

    public static string ToUnixSeparator(string path) => path
        .Replace('\\', '/')
        .TrimStart('/');
}