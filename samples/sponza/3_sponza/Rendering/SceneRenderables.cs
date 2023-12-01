using System.Collections.Concurrent;
using core;
using rendering.components;

namespace sample.Rendering;

public static class SceneRenderables {
    public static RenderableCollection
        CollectRenderables(SceneNode sceneNode, RenderableCollection? previousCollection) {
        var concurrentBag = new ConcurrentBag<RenderableItem>();

        Task.Factory.StartNew(() => Traverse(sceneNode, concurrentBag, previousCollection)).Wait();

        return new(concurrentBag);
    }

    private static void Traverse(SceneNode sceneNode, ConcurrentBag<RenderableItem> bag,
        RenderableCollection? previousCollection) {
        Task.Factory.StartNew(() => {
            var transform = sceneNode.Components.OfType<TransformComponent>().FirstOrDefault();
            var renderable = sceneNode.Components.OfType<IRenderableComponent>().FirstOrDefault();

            if (transform is null || renderable is null) {
                return;
            }

            if (previousCollection?.Lookup.TryGetValue(sceneNode, out var previousItem) ?? false) {
                bag.Add(previousItem);
                return;
            }

            var item = new RenderableItem(
                in sceneNode,
                in transform,
                in renderable
            );

            bag.Add(item);
        }, TaskCreationOptions.AttachedToParent);

        foreach (var child in sceneNode.Children) {
            Task.Factory.StartNew(() => Traverse(child, bag, previousCollection), TaskCreationOptions.AttachedToParent);
        }
    }
}