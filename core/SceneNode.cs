using System.Collections.Immutable;

namespace core;

public record SceneNode {
    public SceneNode(
        in SceneNode? parent,
        string? name = default,
        ImmutableList<SceneNode>? children = null,
        ImmutableList<Component>? components = null
    ) {
        Parent = parent;
        Name = name;
        Children = children ?? ImmutableList<SceneNode>.Empty;
        Components = components ?? ImmutableList<Component>.Empty;
    }

    public readonly SceneNode? Parent;
    public readonly string? Name;
    public volatile ImmutableList<SceneNode> Children;
    public volatile ImmutableList<Component> Components;
}