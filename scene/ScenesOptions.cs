namespace scene;

public record struct SceneDefinition(
    Type Scene,
    Dictionary<string, SceneDefinition>? Scenes = null
);

public record ScenesOptions {
    public required string InitialScene { get; set; }
    public required Dictionary<string, SceneDefinition> Scenes { get; set; }
}