using Vortice.Direct3D12;

namespace window.graphics;
public class RenderTargetView(
    Device device,
    DescriptorHeap heap,
    Texture texture,
    RenderTargetViewDescription? description
) : ResourceView<Texture>(
    texture,
    CreateViewHandle(
        device,
        heap,
        texture,
        description
    )
) {
    private static CpuDescriptorHandle CreateViewHandle(
        Device device,
        DescriptorHeap heap,
        Texture texture,
        RenderTargetViewDescription? description
    ) {
        var cpuHandle = heap.Allocate(1);
        device.DXDevice.CreateRenderTargetView(
            texture.DXResource,
            description,
            cpuHandle
        );
        return cpuHandle;
    }
}