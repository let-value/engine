using Vortice.Direct3D12;

namespace graphics;

public record GraphicsResource : IDisposable {
    public ResourceDescription Description;
    public ID3D12Resource NativeResource;

    public GraphicsResource(ID3D12Resource resource) {
        NativeResource = resource;
        Description = resource.Description;
    }

    public GraphicsResource(
        GraphicsDevice device,
        ResourceDescription description,
        HeapType heapType,
        ClearValue? clearValue = null
    ) {
        var resourceStates = heapType switch {
            HeapType.Upload => ResourceStates.GenericRead,
            HeapType.Readback => ResourceStates.CopyDest,
            _ => ResourceStates.Common
        };

        var resource = device.NativeDevice.CreateCommittedResource(
            new HeapProperties(heapType),
            HeapFlags.None,
            description,
            resourceStates,
            clearValue
        );

        Description = description;
        NativeResource = resource;
    }

    public ulong Width => Description.Width;
    public int Height => Description.Height;
    public ulong SizeInBytes => Description.Width * (ulong)Description.Height * Description.DepthOrArraySize;

    public void Dispose() {
        NativeResource.Dispose();
    }
}