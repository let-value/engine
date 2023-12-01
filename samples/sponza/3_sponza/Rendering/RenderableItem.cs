using core;
using rendering.components;

namespace sample.Rendering;

public class RenderableItem {
    public SceneNode Node;
    public TransformComponent Transform;
    public IRenderableComponent Renderable;

    public RenderableItem(
        in SceneNode node,
        in TransformComponent transform,
        in IRenderableComponent renderable
    ) {
        Node = node;
        Transform = transform;
        Renderable = renderable;
    }
}