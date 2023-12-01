using assets;
using assets.assets;
using core;
using rendering.components;
using System.Collections.Immutable;

namespace scene;

public class SceneImporter {
    public SceneNode ImportModelAsset(ModelAsset asset) {
        return ConvertAssimpNode(asset, asset.Scene.RootNode, null);
    }

    private SceneNode ConvertAssimpNode(ModelAsset asset, Assimp.Node assimpNode, in SceneNode? parent) {
        var node = new SceneNode(in parent, assimpNode.Name);
        var children = ImmutableList.CreateBuilder<SceneNode>();

        node.Components = ConvertAssimpNodeToComponents(asset, assimpNode).ToImmutable();

        foreach (var child in assimpNode.Children) {
            children.Add(ConvertAssimpNode(asset, child, in node));
        }

        node.Children = children.ToImmutable();

        return node;
    }

    private ImmutableList<Component>.Builder ConvertAssimpNodeToComponents(ModelAsset asset, Assimp.Node assimpNode) {
        var components = ImmutableList.CreateBuilder<Component>();


        components.Add(new TransformComponent(assimpNode.Transform.ToNumerics()));

        foreach (var meshIndex in assimpNode.MeshIndices) {
            components.Add(new RenderableMeshComponent(asset.Meshes[meshIndex]));
        }

        return components;
    }
}