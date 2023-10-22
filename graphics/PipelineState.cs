using Vortice.Direct3D12;

namespace graphics;

public record struct PipelineState : IDisposable {
    public ID3D12PipelineState NativePipelineState;

    public PipelineState(
        GraphicsDevice device,
        GraphicsPipelineStateDescription pipelineStateDescription
    ) {
        NativePipelineState = device.NativeDevice.CreateGraphicsPipelineState(pipelineStateDescription);
    }

    public readonly void Dispose() {
        NativePipelineState.Dispose();
    }
}