using System;
using Vortice.Direct3D12;

namespace window.graphics;

public class Resource(ID3D12Resource dxResource) : IDisposable {
    public ResourceDescription Description = dxResource.Description;
    public ID3D12Resource DXResource = dxResource;

    public ulong Width => Description.Width;
    public int Height => Description.Height;
    public ulong SizeInBytes => Description.Width * (ulong)Description.Height * Description.DepthOrArraySize;

    public void Dispose() {
        DXResource.Dispose();
    }

    public static Resource CreateCommittedResource(
        Device device,
        ResourceDescription description,
        HeapType heapType,
        ClearValue? clearValue = null
    ) {
        var resourceStates = heapType switch {
            HeapType.Upload => ResourceStates.GenericRead,
            HeapType.Readback => ResourceStates.CopyDest,
            _ => ResourceStates.Common
        };

        var resource = device.DXDevice.CreateCommittedResource(
            new HeapProperties(heapType),
            HeapFlags.None,
            description,
            resourceStates,
            clearValue
        );

        return new(resource);
    }
}
