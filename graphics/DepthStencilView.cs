namespace graphics;

public record DepthStencilView : ResourceView
{
    public DepthStencilView(
        GraphicsDevice device,
        DescriptorAllocator allocator,
        GraphicsResource resource
    )
    {
        var cpuHandle = allocator.Allocate(1);
        device.NativeDevice.CreateDepthStencilView(
            resource.NativeResource,
            null,
            cpuHandle
        );

        Resource = resource;
        CpuDescriptor = cpuHandle;
    }
}