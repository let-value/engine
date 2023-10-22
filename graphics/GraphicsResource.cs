using Vortice.Direct3D12;

namespace graphics;

public record GraphicsResource : IDisposable {
    public ID3D12Resource NativeResource;
    public ResourceDescription Description;
    public ulong Width => Description.Width;
    public int Height => Description.Height;
    public ulong SizeInBytes => Description.Width * (ulong)Description.Height * Description.DepthOrArraySize;

    public GraphicsResource(ID3D12Resource resource) {
        NativeResource = resource;
        Description = resource.Description;
    }

    public GraphicsResource(
        GraphicsDevice device,
        ResourceDescription description,
        HeapType heapType
    ) {
        var resourceStates = heapType switch {
            HeapType.Upload => ResourceStates.GenericRead,
            HeapType.Readback => ResourceStates.CopyDest,
            _ => ResourceStates.Common,
        };

        var resource = device.NativeDevice.CreateCommittedResource(
            new HeapProperties(heapType),
            HeapFlags.None,
            description,
            resourceStates
        );

        Description = description;
        NativeResource = resource;
    }

    public void Dispose() {
        NativeResource.Dispose();
    }
}