global using ModelScene = Assimp.Scene;
using assets.resources;

namespace assets.assets;

public class ModelAsset : IAsset {
    public required object Key { get; set; }
    public required ModelScene Scene;
    public required IndexedDictionary<string, TextureImageResource> Textures;
    public required IndexedDictionary<string, MaterialResource> Materials;
    public required IndexedDictionary<string, MeshResource> Meshes;
}