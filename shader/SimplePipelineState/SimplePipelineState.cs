using graphics;
using Vortice.Direct3D12;
using Vortice.Dxc;
using Vortice.DXGI;

namespace shader.SimplePipelineState;

public class SimplePipelineState {
    public readonly PipelineState PipelineState;
    public readonly RootSignature RootSignature;

    public SimplePipelineState(GraphicsDevice device) {
        var rootSignatureFlags = RootSignatureFlags.AllowInputAssemblerInputLayout
                                 | RootSignatureFlags.DenyHullShaderRootAccess
                                 | RootSignatureFlags.DenyDomainShaderRootAccess
                                 | RootSignatureFlags.DenyGeometryShaderRootAccess
                                 | RootSignatureFlags.DenyAmplificationShaderRootAccess
                                 | RootSignatureFlags.DenyMeshShaderRootAccess;

        RootSignature = new(device, new(rootSignatureFlags));

        var inputElementDescs = new InputLayoutDescription(
            new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElementDescription("COLOR", 0, Format.R32G32B32A32_Float, 12, 0)
        );

        var vertexShaderByteCode = Utils.CompileBytecode(DxcShaderStage.Vertex, "Triangle.hlsl", "VSMain");
        var pixelShaderByteCode = Utils.CompileBytecode(DxcShaderStage.Pixel, "Triangle.hlsl", "PSMain");

        var pipelineStateDescription = new GraphicsPipelineStateDescription {
            RootSignature = RootSignature.NativeRootSignature,
            VertexShader = vertexShaderByteCode,
            PixelShader = pixelShaderByteCode,
            InputLayout = inputElementDescs,
            SampleMask = uint.MaxValue,
            PrimitiveTopologyType = PrimitiveTopologyType.Triangle,
            RasterizerState = RasterizerDescription.CullCounterClockwise,
            BlendState = BlendDescription.Opaque,
            DepthStencilState = DepthStencilDescription.Default,
            RenderTargetFormats = new[] { Format.R8G8B8A8_UNorm },
            DepthStencilFormat = Format.D32_Float,
            SampleDescription = SampleDescription.Default
        };

        PipelineState = new(device, pipelineStateDescription);
    }


    public void Dispose() {
        RootSignature.Dispose();
        PipelineState.Dispose();
    }
}