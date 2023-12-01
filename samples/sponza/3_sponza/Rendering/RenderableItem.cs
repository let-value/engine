using System.Numerics;
using core;
using rendering.components;

namespace sample.Rendering;

public class RenderableItem(
    in SceneNode node,
    in Matrix4x4 transform,
    in IRenderableComponent renderable
) {
    public SceneNode Node = node;
    public Matrix4x4 Transform = transform;
    public IRenderableComponent Renderable = renderable;
}