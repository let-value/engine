using System;
using Vortice.Direct3D12;

namespace window.graphics;

public class PipelineState(
    Device device,
    GraphicsPipelineStateDescription pipelineStateDescription
) : IDisposable {
    public ID3D12PipelineState DXPipelineState =
        device.DXDevice.CreateGraphicsPipelineState(pipelineStateDescription);

    public void Dispose() {
        DXPipelineState.Dispose();
    }
}