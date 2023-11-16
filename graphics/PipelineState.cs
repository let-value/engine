using Vortice.Direct3D12;

namespace graphics;

public class PipelineState(
        GraphicsDevice device,
        GraphicsPipelineStateDescription pipelineStateDescription
    )
    : IDisposable {
    public ID3D12PipelineState NativePipelineState =
        device.NativeDevice.CreateGraphicsPipelineState(pipelineStateDescription);

    public void Dispose() {
        NativePipelineState.Dispose();
    }
}