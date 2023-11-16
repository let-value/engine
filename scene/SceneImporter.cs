using System.Collections.Immutable;
using assets;
using assets.assets;
using scene.components;

namespace scene;

public class SceneImporter {
    public SceneNode ImportModelAsset(ModelAsset asset, in SceneNode parentNode) {
        return parentNode.AddChildren(ConvertAssimpNode(asset, asset.Scene.RootNode));
    }

    private SceneNode ConvertAssimpNode(ModelAsset asset, Assimp.Node assimpNode) {
        var children = ImmutableList.CreateBuilder<SceneNode>();
        var components = ConvertAssimpNodeToComponents(asset, assimpNode);

        foreach (var child in assimpNode.Children) {
            children.Add(ConvertAssimpNode(asset, child));
        }

        return new(children.ToImmutable(), components.ToImmutable());
    }

    private ImmutableList<Component>.Builder ConvertAssimpNodeToComponents(ModelAsset asset, Assimp.Node assimpNode) {
        var components = ImmutableList.CreateBuilder<Component>();

        assimpNode.Transform.Decompose(out var scale, out var rotation, out var translation);
        components.Add(new TransformComponent(scale.ToNumerics(), rotation.ToNumerics(), translation.ToNumerics()));

        foreach (var meshIndex in assimpNode.MeshIndices) {
            components.Add(new RenderableMesh(asset.Meshes[meshIndex]));
        }

        return components;
    }
}