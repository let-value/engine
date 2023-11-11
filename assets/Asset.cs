global using AssetScene = Assimp.Scene;
using assets.resources;

namespace assets;

public class Asset {
    public required AssetScene Scene;
    public required IndexedDictionary<string, TextureImageResource> Textures;
    public required IndexedDictionary<string, MaterialResource> Materials;
    public required IndexedDictionary<string, MeshResource> Meshes;
}