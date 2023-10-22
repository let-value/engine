using Vortice.Direct3D12;

namespace graphics;

public record ResourceView : IDisposable {
    public CpuDescriptorHandle CpuDescriptor;
    public GraphicsResource Resource;

    public void Dispose() {
        Resource.Dispose();
    }
}