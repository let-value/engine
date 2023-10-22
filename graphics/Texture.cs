using Vortice.Direct3D12;
using Vortice.DXGI;

namespace graphics;

public record Texture : GraphicsResource {
    //public Texture(GraphicsDevice device, ResourceDescription description, HeapType heapType)
    //    : base(device, description, heapType)
    //{
    //}

    public Texture(ID3D12Resource resource) : base(resource) { }

    public Texture(
        GraphicsDevice device,
        ResourceDescription description,
        HeapType heapType,
        ClearValue? clearValue = null
    ) : base(
        device, description, heapType, clearValue
    ) { }

    public static Texture Create2D(
        GraphicsDevice device,
        uint width,
        uint height,
        Format format,
        ResourceFlags textureFlags = ResourceFlags.None,
        ushort mipLevels = 1,
        ushort arraySize = 1,
        int sampleCount = 1,
        int sampleQuality = 0,
        HeapType heapType = HeapType.Default,
        ClearValue? clearValue = null
    ) {
        return new(
            device,
            ResourceDescription.Texture2D(
                format,
                width,
                height,
                arraySize,
                mipLevels,
                sampleCount,
                sampleQuality,
                textureFlags
            ),
            heapType,
            clearValue
        );
    }
}