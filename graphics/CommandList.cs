using System.Runtime.InteropServices;
using Vortice.Direct3D12;

namespace graphics;

[StructLayout(LayoutKind.Sequential)]
public record struct CommandList : IDisposable {
    public ID3D12GraphicsCommandList4 NativeCommandList;
    public ID3D12CommandAllocator NativeCommandAllocator;

    public CommandList(GraphicsDevice device, CommandListType type, PipelineState? pipelineState = null) {
        NativeCommandAllocator = device.NativeDevice.CreateCommandAllocator(type);
        NativeCommandList = device.NativeDevice.CreateCommandList<ID3D12GraphicsCommandList4>(
            type,
            NativeCommandAllocator,
            pipelineState?.NativePipelineState
        );
    }

    public void Dispose() {
        NativeCommandAllocator.Dispose();
        NativeCommandList.Dispose();
    }

    public void Close() {
        NativeCommandList.Close();
    }

    public void Reset(PipelineState? pipelineState = null) {
        NativeCommandAllocator.Reset();
        NativeCommandList.Reset(NativeCommandAllocator, pipelineState?.NativePipelineState);
    }
}

public class CommandListFactory(GraphicsDevice device) {
    public CommandList Create(CommandListType type, PipelineState? pipelineState = null) {
        return new(device, type, pipelineState);
    }
}