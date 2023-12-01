using SixLabors.ImageSharp;

namespace assets.resources;

public class TextureImageResource {
    public required Image? Image;
}

public class TextureResource : TextureImageResource {
    public Assimp.TextureSlot Slot;
}