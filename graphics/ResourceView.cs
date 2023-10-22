using Vortice.Direct3D12;

namespace graphics;

public record ResourceView : IDisposable
{
    public GraphicsResource Resource;
    public CpuDescriptorHandle CpuDescriptor;

    public void Dispose()
    {
        Resource.Dispose();
    }
}