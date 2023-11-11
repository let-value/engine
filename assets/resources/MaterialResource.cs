namespace assets.resources;

public class MaterialResource {
    public required Assimp.Material Material;
    public required Dictionary<Assimp.TextureType, TextureResource> Textures;
}