using System;
using Vortice.Direct3D12;

namespace window.graphics;

public class ResourceView<TResource>(
    TResource resource,
    CpuDescriptorHandle cpuDescriptor
) : IDisposable where TResource : Resource {
    public TResource Resource = resource;
    public CpuDescriptorHandle CpuDescriptor = cpuDescriptor;

    public void Dispose() {
        Resource.Dispose();
    }
}