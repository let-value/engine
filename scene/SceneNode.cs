using System.Collections.Immutable;
using scene.components;

namespace scene;

public record SceneNode(
    ImmutableList<SceneNode>? Children = null,
    ImmutableList<Component>? Components = null
) {
    public ImmutableList<SceneNode> Children { get; init; } = Children ?? ImmutableList<SceneNode>.Empty;
    public ImmutableList<Component> Components { get; init; } = Components ?? ImmutableList<Component>.Empty;
    public SceneNode AddChildren(SceneNode child) => this with { Children = Children.Add(child) };
    public SceneNode AddComponent(Component component) => this with { Components = Components.Add(component) };
}