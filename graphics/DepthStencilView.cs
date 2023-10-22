using Vortice.Direct3D12;

namespace graphics;

public record DepthStencilView : ResourceView {
    public DepthStencilView(
        GraphicsDevice device,
        DescriptorAllocator allocator,
        GraphicsResource resource,
        DepthStencilViewDescription? description = null
    ) {
        var cpuHandle = allocator.Allocate(1);
        device.NativeDevice.CreateDepthStencilView(
            resource.NativeResource,
            description,
            cpuHandle
        );

        Resource = resource;
        CpuDescriptor = cpuHandle;
    }
}