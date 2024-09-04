using System;
using Vortice.Direct3D12;

namespace window.graphics;

public class DescriptorHeap : IDisposable {
    public DescriptorHeapDescription Description;
    public ID3D12DescriptorHeap DXDescriptorHeap;
    public CpuDescriptorHandle CpuDescriptor;
    public int CurrentDescriptorCount = 0;

    private readonly int incrementSize;

    public DescriptorHeap(
        Device device,
        DescriptorHeapDescription description
    ) {
        Description = description;
        incrementSize = device.DXDevice.GetDescriptorHandleIncrementSize(description.Type);
        DXDescriptorHeap = device.DXDevice.CreateDescriptorHeap(description);
        CpuDescriptor = DXDescriptorHeap.GetCPUDescriptorHandleForHeapStart();
    }

    public void Dispose() {
        DXDescriptorHeap.Dispose();
    }

    public CpuDescriptorHandle Allocate(int count) {
        var descriptor = CpuDescriptor
            .Offset(CurrentDescriptorCount, incrementSize);

        CurrentDescriptorCount += count;

        return descriptor;
    }
}