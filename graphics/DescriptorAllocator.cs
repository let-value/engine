using Microsoft.Extensions.DependencyInjection;
using Vortice.Direct3D12;

namespace graphics;

public record DescriptorAllocator : IDisposable {
    public const int DescriptorsPerHeap = 4096;
    private readonly object allocatorLock = new();

    public DescriptorAllocator(
        GraphicsDevice device,
        DescriptorHeapType descriptorHeapType,
        int descriptorCount = DescriptorsPerHeap,
        DescriptorHeapFlags descriptorHeapFlags = DescriptorHeapFlags.None
    ) {
        if (descriptorCount < 1 || descriptorCount > DescriptorsPerHeap) {
            throw new ArgumentOutOfRangeException(
                nameof(descriptorCount),
                $"Descriptor count must be between 1 and {DescriptorsPerHeap}."
            );
        }

        Type = descriptorHeapType;
        Flags = descriptorHeapFlags;

        DescriptorHandleIncrementSize = device.NativeDevice.GetDescriptorHandleIncrementSize(descriptorHeapType);

        var descriptorHeapDescription =
            new DescriptorHeapDescription(descriptorHeapType, descriptorCount, descriptorHeapFlags);

        DescriptorHeap = device.NativeDevice.CreateDescriptorHeap(descriptorHeapDescription);

        DescriptorCapacity = descriptorCount;
    }

    public DescriptorHeapType Type { get; set; }
    public DescriptorHeapFlags Flags { get; set; }
    public int DescriptorHandleIncrementSize { get; set; }
    public ID3D12DescriptorHeap DescriptorHeap { get; set; }
    public int DescriptorCapacity { get; set; }

    public int CurrentDescriptorCount { get; set; }

    public void Dispose() {
        DescriptorHeap.Dispose();
    }

    public CpuDescriptorHandle Allocate(int count) {
        lock (allocatorLock) {
            if (count < 1 || count > DescriptorCapacity) {
                throw new ArgumentOutOfRangeException(
                    nameof(count),
                    "Count must be between 1 and the total descriptor count."
                );
            }

            if (CurrentDescriptorCount + count > DescriptorCapacity) {
                Reset();
            }

            var descriptor = DescriptorHeap.GetCPUDescriptorHandleForHeapStart()
                .Offset(CurrentDescriptorCount, DescriptorHandleIncrementSize);

            CurrentDescriptorCount += count;

            return descriptor;
        }
    }

    public GpuDescriptorHandle GetGpuDescriptorHandle(CpuDescriptorHandle descriptor) {
        if (!Flags.HasFlag(DescriptorHeapFlags.ShaderVisible)) {
            throw new InvalidOperationException();
        }

        var cpuStart = DescriptorHeap.GetCPUDescriptorHandleForHeapStart();
        var gpuStart = DescriptorHeap.GetGPUDescriptorHandleForHeapStart();

        return gpuStart.Offset((int)descriptor.Ptr - (int)cpuStart.Ptr);
    }

    public void Reset() {
        CurrentDescriptorCount = 0;
    }
}

public class DescriptorAllocatorFactory(GraphicsDevice device) {
    public DescriptorAllocator Create(
        DescriptorHeapType descriptorHeapType,
        int descriptorCount = DescriptorAllocator.DescriptorsPerHeap,
        DescriptorHeapFlags descriptorHeapFlags = default
    ) {
        return new(device, descriptorHeapType, descriptorCount, descriptorHeapFlags);
    }

    public static Func<IServiceProvider, object?, DescriptorAllocator> Factory(
        int descriptorCount = DescriptorAllocator.DescriptorsPerHeap,
        DescriptorHeapFlags descriptorHeapFlags = default
    ) {
        return (serviceProvider, serviceKey) => {
            if (serviceKey is not DescriptorHeapType descriptorHeapType) throw new InvalidOperationException();

            var factory = serviceProvider.GetRequiredService<DescriptorAllocatorFactory>();
            return factory.Create(descriptorHeapType, descriptorCount, descriptorHeapFlags);
        };
    }
}