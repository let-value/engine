using System.Numerics;
using core;
using rendering.components;

namespace sample.Rendering;

public static class SceneRenderables {
    public static RenderableCollection CollectRenderables(SceneNode sceneNode) {
        var items = new List<RenderableItem>();

        var transform = Matrix4x4.Identity;
        Traverse(sceneNode, items, ref transform);

        return new(items);
    }

    private static void Traverse(
        in SceneNode sceneNode,
        ICollection<RenderableItem> list,
        ref Matrix4x4 parentTransform
    ) {
        var transformComponent = sceneNode.Components.OfType<TransformComponent>().FirstOrDefault();
        var transform = parentTransform;
        if (transformComponent is not null) {
            transform *= transformComponent.Value;
        }

        var renderable = sceneNode.Components.OfType<IRenderableComponent>().FirstOrDefault();

        if (renderable is not null) {
            var item = new RenderableItem(
                in sceneNode,
                in transform,
                in renderable
            );

            list.Add(item);
        }

        foreach (var child in sceneNode.Children) {
            Traverse(child, list, ref transform);
        }
    }
}