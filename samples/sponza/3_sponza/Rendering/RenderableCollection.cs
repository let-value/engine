using assets.resources;
using rendering.components;
using RenderableItemMesh = (
    sample.Rendering.RenderableItem Item,
    rendering.components.RenderableMeshComponent Component
    );

namespace sample.Rendering;

public class RenderableCollection(IEnumerable<RenderableItem> items) {
    public List<RenderableItem> Items = [..items];

    public IEnumerable<RenderableItemMesh> Meshes => Items
        //.AsParallel()
        .Where(item => item.Renderable is RenderableMeshComponent)
        .Select(item => new RenderableItemMesh(item, (RenderableMeshComponent)item.Renderable));
}

public static class RenderableCollectionMeshesExtensions {
    public static IEnumerable<IGrouping<MeshResource, RenderableItemMesh>> GroupMeshes(
        this IEnumerable<RenderableItemMesh> renderableMeshes) =>
        renderableMeshes
            //.AsParallel()
            .GroupBy(item => item.Component.Mesh);

    public static IEnumerable<RenderableItemMesh>
        GetOpaqueMeshes(this IEnumerable<RenderableItemMesh> renderableMeshes) =>
        renderableMeshes
            //.AsParallel()
            .Where(item => item.Component.Mesh.Material.Material.Opacity == 0);

    public static IEnumerable<RenderableItemMesh> GetTransparentMeshes(
        this IEnumerable<RenderableItemMesh> renderableMeshes) =>
        renderableMeshes
            //.AsParallel()
            .Where(item => item.Component.Mesh.Material.Material.Opacity > 0);

    public static IEnumerable<IGrouping<MeshResource, RenderableItemMesh>> GetInstancedMeshes(
        this IEnumerable<RenderableItemMesh> renderableMeshes) =>
        renderableMeshes
            .GroupMeshes()
            //.AsParallel()
            .Where(group => group.Count() > 1);

    public static IEnumerable<RenderableItemMesh> GetMeshes(this IEnumerable<RenderableItemMesh> renderableMeshes) =>
        renderableMeshes
            .GroupMeshes()
            //.AsParallel()
            .Where(group => group.Count() < 2)
            .SelectMany(group => group);

    public static IEnumerable<IGrouping<MaterialResource, RenderableItemMesh>> GetMaterialMeshes(
        this IEnumerable<RenderableItemMesh> renderableMeshes) =>
        renderableMeshes
            //.AsParallel()
            .GroupBy(item => item.Component.Mesh.Material);
}