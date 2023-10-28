using graphics;
using rendering;
using shader;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.Dxc;
using Vortice.DXGI;

namespace sample;

public class TriangleRenderingPipeline : IRenderPipeline {
    private readonly CommandList CommandList;

    private readonly RootSignature RootSignature;
    private readonly PipelineState PipelineState;

    private readonly GraphicsResource VertexBuffer;
    private readonly VertexBufferView VertexBufferView;

    public TriangleRenderingPipeline(
        GraphicsDevice device,
        CommandListFactory commandListFactory
    ) {
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

        var vertexShaderByteCode = ShaderUtils.CompileBytecode(DxcShaderStage.Vertex, "Triangle.hlsl", "VSMain");
        var pixelShaderByteCode = ShaderUtils.CompileBytecode(DxcShaderStage.Pixel, "Triangle.hlsl", "PSMain");

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

        CommandList = commandListFactory.Create(CommandListType.Direct, PipelineState);
        CommandList.Close();

        var scale = 0.6f;

        var triangleVertices = new[] {
            new VertexPositionColor(new(0f, scale, 0.0f), new(1.0f, 0.0f, 0.0f, 1.0f)),
            new VertexPositionColor(new(scale, -scale, 0.0f), new(0.0f, 1.0f, 0.0f, 1.0f)),
            new VertexPositionColor(new(-scale, -scale, 0.0f), new(0.0f, 0.0f, 1.0f, 1.0f))
        };

        var vertexBufferSize = triangleVertices.Length * VertexPositionColor.SizeInBytes;

        VertexBuffer = new(device, ResourceDescription.Buffer(vertexBufferSize), HeapType.Upload);
        VertexBuffer.NativeResource.SetData(triangleVertices);

        VertexBufferView = new(
            VertexBuffer.NativeResource.GPUVirtualAddress,
            vertexBufferSize,
            VertexPositionColor.SizeInBytes
        );
    }

    public CommandList[] Render(FrameContext frameContext) {
        var (_, renderTargetView, depthStencilView, viewport, scissorRect) = frameContext;

        CommandList.Reset(PipelineState);
        CommandList.NativeCommandList.SetGraphicsRootSignature(RootSignature.NativeRootSignature);

        CommandList.NativeCommandList.OMSetRenderTargets(
            renderTargetView.CpuDescriptor,
            depthStencilView.CpuDescriptor
        );

        CommandList.NativeCommandList.RSSetViewport(viewport);
        CommandList.NativeCommandList.RSSetScissorRect(scissorRect);

        CommandList.NativeCommandList.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
        CommandList.NativeCommandList.IASetVertexBuffers(0, VertexBufferView);
        CommandList.NativeCommandList.DrawInstanced(3, 1, 0, 0);
        CommandList.Close();

        return new[] { CommandList };
    }

    public void Dispose() {
        CommandList.Dispose();
        VertexBuffer.Dispose();
    }
}