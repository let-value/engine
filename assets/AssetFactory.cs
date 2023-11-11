using assets.resources;
using Assimp;
using Microsoft.Extensions.FileProviders;

namespace assets;

public class AssetFactory(AssimpContext loader) {
    public Asset Create(string path) {
        var directory = Path.Combine(
            Directory.GetCurrentDirectory(),
            Path.GetDirectoryName(path) ?? throw new InvalidOperationException()
        );

        using var fileProvider = new PhysicalFileProvider(directory ?? throw new InvalidOperationException());

        var scene = loader.ImportFile(path);

        var textures = LoadTextureImages(scene);
        var materials = LoadMaterials(scene, textures, fileProvider);
        var meshes = LoadMeshes(scene, materials);

        return new() {
            Scene = scene,
            Textures = textures,
            Materials = materials,
            Meshes = meshes
        };
    }

    private IndexedDictionary<string, TextureImageResource> LoadTextureImages(Scene scene) {
        return !scene.HasTextures ? new() : throw new NotImplementedException();
    }

    private IndexedDictionary<string, MaterialResource> LoadMaterials(
        Scene scene,
        IndexedDictionary<string, TextureImageResource> textures,
        IFileProvider fileProvider
    ) {
        if (!scene.HasMaterials) {
            return new();
        }

        var materials = new IndexedDictionary<string, MaterialResource>(scene.Materials.Count);

        foreach (var material in scene.Materials) {
            var textureSlots = material.GetAllMaterialTextures();
            var materialTextures = new Dictionary<TextureType, TextureResource>(textureSlots.Length);

            foreach (var textureSlot in textureSlots) {
                if (textureSlot.FilePath is null) {
                    throw new NotImplementedException();
                }

                var ok = textures.TryGetValue(textureSlot.FilePath, out var textureImageResource);
                if (!ok) {
                    var textureImage = LoadTextureImage(textureSlot, fileProvider);
                    if (textureImage != null) {
                        textureImageResource = textureImage;
                        textures.Add(textureSlot.FilePath, textureImageResource);
                    }
                }

                var textureResource = new TextureResource {
                    Image = textureImageResource.Image,
                    Slot = textureSlot
                };

                materialTextures.Add(textureSlot.TextureType, textureResource);
            }

            materials.Add(material.Name, new() {
                Material = material,
                Textures = materialTextures
            });
        }

        return materials;
    }

    private TextureImageResource? LoadTextureImage(TextureSlot textureSlot, IFileProvider fileProvider) {
        var fileInfo = fileProvider.GetFileInfo(textureSlot.FilePath);
        if (!fileInfo.Exists) {
            return null;
        }

        using var stream = fileInfo.CreateReadStream();
        var image = Image.Load(stream);
        return new() {
            Image = image
        };
    }

    private IndexedDictionary<string, MeshResource> LoadMeshes(
        Scene scene,
        IndexedDictionary<string, MaterialResource> materials
    ) {
        if (!scene.HasMeshes) {
            return new();
        }

        var meshes = new IndexedDictionary<string, MeshResource>(scene.Meshes.Count);

        foreach (var mesh in scene.Meshes) {
            var material = mesh.MaterialIndex >= 0 ? materials[mesh.MaterialIndex] : null;
            var meshResource = new MeshResource {
                Mesh = mesh,
                Material = material
            };

            meshes.Add(mesh.Name, meshResource);
        }

        return meshes;
    }
}