using Vortice.Direct3D12;

namespace graphics;

public class RenderTargetView : ResourceView {
    public RenderTargetView(
        GraphicsDevice device,
        DescriptorAllocator allocator,
        GraphicsResource resource,
        RenderTargetViewDescription? description
    ) {
        var cpuHandle = allocator.Allocate(1);
        device.NativeDevice.CreateRenderTargetView(
            resource.NativeResource,
            description,
            cpuHandle
        );

        Resource = resource;
        CpuDescriptor = cpuHandle;
    }
}