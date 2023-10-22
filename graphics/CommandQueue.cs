using Microsoft.Extensions.DependencyInjection;
using Vortice.Direct3D12;

namespace graphics;

public record CommandQueue : IDisposable {
    public ID3D12Fence NativeFence;
    public ID3D12CommandQueue NativeQueue;
    private ulong NextFenceValue = 1;

    public CommandQueue(GraphicsDevice device, CommandListType type) {
        NativeFence = device.NativeDevice.CreateFence();
        NativeQueue = device.NativeDevice.CreateCommandQueue(type);
    }

    public void Execute(params CommandList[] commandLists) {
        if (!commandLists.Any()) {
            return;
        }

        var fenceValue = ExecuteInternal(commandLists);

        WaitForFence(NativeFence, fenceValue);
    }

    public void ExecuteSimple(params CommandList[] commandLists) {
        if (!commandLists.Any()) {
            return;
        }

        var nativeCommandLists = commandLists.Select(commandList => commandList.NativeCommandList).ToArray();
        NativeQueue.ExecuteCommandLists(nativeCommandLists);
    }

    private ulong ExecuteInternal(params CommandList[] commandLists) {
        var fenceValue = NextFenceValue++;

        var nativeCommandLists = commandLists.Select(commandList => commandList.NativeCommandList).ToArray();
        NativeQueue.ExecuteCommandLists(nativeCommandLists);
        NativeQueue.Signal(NativeFence, fenceValue);

        return fenceValue;
    }

    private bool IsFenceComplete(ID3D12Fence fence, ulong fenceValue) {
        return fence.CompletedValue >= fenceValue;
    }

    private void WaitForFence(ID3D12Fence fence, ulong fenceValue) {
        if (IsFenceComplete(fence, fenceValue)) {
            return;
        }

        using var fenceEvent = new ManualResetEvent(false);
        fence.SetEventOnCompletion(fenceValue, fenceEvent);

        fenceEvent.WaitOne();
    }

    public void Dispose() {
        NativeQueue.Dispose();
    }
}

public class CommandQueueFactory(GraphicsDevice device) {
    public CommandQueue Create(CommandListType type) {
        return new(device, type);
    }

    public static CommandQueue Factory(IServiceProvider serviceProvider, object? serviceKey) {
        if (serviceKey is not CommandListType commandListType) throw new InvalidOperationException();

        var factory = serviceProvider.GetRequiredService<CommandQueueFactory>();
        return factory.Create(commandListType);
    }
}