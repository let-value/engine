using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace scene;

record FlatScene(string Path, SceneDefinition Definition);

public record StackScene(string Path, IScene Instance, IServiceScope Scope);

public delegate void ForegroundChangedHandler(object sender, EventArgs e);

public class SceneManager {
    private readonly IServiceProvider Services;
    private readonly List<FlatScene> Scenes;

    public string Location = "";
    public readonly Stack<StackScene> SceneStack = new();

    public ReadOnlyCollection<IScene> ForegroundScenes = new(Array.Empty<IScene>());
    public event ForegroundChangedHandler? ForegroundChanged;

    public SceneManager(IOptions<ScenesOptions> scenesOptions, IServiceProvider services) {
        Services = services;
        Scenes = FlattenSceneDictionary(scenesOptions.Value.Scenes);

        NavigateTo(scenesOptions.Value.InitialScene);
    }

    public void NavigateTo(string path) {
        var newLocation = ToUnixSeparator(Path.Combine(ToSystemSeparator(Location), ToSystemSeparator(path)));

        UpdateSceneStack(newLocation);

        Location = newLocation;
    }

    private void UpdateSceneStack(string newLocation) {
        var toDispose = new List<StackScene>();
        while (SceneStack.Count > 0 && !newLocation.StartsWith(SceneStack.Peek().Path)) {
            var scene = SceneStack.Pop();
            toDispose.Add(scene);
        }

        foreach (var scene in Scenes) {
            if (!newLocation.StartsWith(scene.Path)) {
                continue;
            }

            if (SceneStack.Count > 0 && SceneStack.Peek().Path == scene.Path) {
                continue;
            }

            var scope = Services.CreateScope();

            if (scope.ServiceProvider.GetRequiredService(scene.Definition.Scene) is not IScene instance) {
                throw new InvalidOperationException($"Scene {scene.Path} is not an IScene");
            }

            SceneStack.Push(new(scene.Path, instance, scope));
        }

        if (SceneStack.Count == 0 || SceneStack.Peek().Path != newLocation) {
            throw new InvalidOperationException($"Scene {newLocation} not found");
        }

        UpdateForegroundScenes();

        foreach (var scene in toDispose) {
            scene.Instance.Dispose();
            scene.Scope.Dispose();
        }
    }

    private void UpdateForegroundScenes() {
        var foregroundScenes = new List<IScene>();

        foreach (var stackScene in SceneStack.Reverse()) {
            foregroundScenes.Add(stackScene.Instance);

            if (!stackScene.Instance.IsModal) {
                break;
            }
        }

        ForegroundScenes = new(foregroundScenes);
        ForegroundChanged?.Invoke(this, EventArgs.Empty);
    }

    private static List<FlatScene> FlattenSceneDictionary(
        Dictionary<string, SceneDefinition> scenes,
        string? parentPath = null
    ) {
        var result = new List<FlatScene>();

        foreach (var kvp in scenes) {
            var combinedPath = ToUnixSeparator(
                string.IsNullOrEmpty(parentPath)
                    ? kvp.Key
                    : Path.Combine(
                        ToSystemSeparator(parentPath),
                        ToSystemSeparator(kvp.Key)
                    )
            );

            result.Add(new(combinedPath, kvp.Value));

            if (kvp.Value.Scenes != null) {
                result.AddRange(FlattenSceneDictionary(kvp.Value.Scenes, combinedPath));
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