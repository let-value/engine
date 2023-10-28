using Vortice.Direct3D12;

namespace rendering;

public class PresenterSynchronizationsContext(PresenterContext context) : IDisposable {
    private readonly ID3D12Fence FrameFence = context.Device.NativeDevice.CreateFence();
    private readonly AutoResetEvent FrameFenceEvent = new(false);
    private readonly SemaphoreSlim ResizeLock = new(1, 1);
    private ulong FrameCount;

    public void Lock() => ResizeLock.Wait();
    public void Release() => ResizeLock.Release();

    public void WaitForGpuCompletion() {
        context.CommandQueue.NativeQueue.Signal(FrameFence, ++FrameCount);

        if (FrameFence.CompletedValue >= FrameCount) {
            return;
        }

        FrameFence.SetEventOnCompletion(FrameCount, FrameFenceEvent);
        FrameFenceEvent.WaitOne();
    }

    public void Dispose() {
        FrameFence.Dispose();
        FrameFenceEvent.Dispose();
    }

    public void WaitForFrameCompletion(int renderLatency) {
        context.CommandQueue.NativeQueue.Signal(FrameFence, ++FrameCount);

        var gpuFrameCount = FrameFence.CompletedValue;
        var elapsed = (int)(FrameCount - gpuFrameCount);
        if (elapsed < renderLatency) {
            return;
        }

        FrameFence.SetEventOnCompletion(gpuFrameCount + 1, FrameFenceEvent);
        FrameFenceEvent.WaitOne();
    }
}