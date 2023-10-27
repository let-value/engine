using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using graphics;
using rendering;
using rendering.components;
using shader.SimplePipelineState;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.Mathematics;

namespace sample;

public class TriangleRenderingPipeline : IRenderPipeline {
    private readonly CommandList CommandList;
    private readonly SimplePipelineState SimplePipelineState;
    private readonly GraphicsResource VertexBuffer;
    private readonly VertexBufferView VertexBufferView;

    public TriangleRenderingPipeline(
        GraphicsDevice device,
        CommandListFactory commandListFactory
    ) {
        SimplePipelineState = new(device);

        CommandList = commandListFactory.Create(CommandListType.Direct, SimplePipelineState.PipelineState);
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

    public CommandList[] Render(int backBufferIndex,
        RenderTargetView renderTargetView,
        DepthStencilView depthStencilView,
        in ReadOnlySpan<IRenderable> renderables,
        Viewport viewport,
        Rectangle scissorRect
    ) {
        CommandList.Reset(SimplePipelineState.PipelineState);
        CommandList.NativeCommandList.SetGraphicsRootSignature(SimplePipelineState.RootSignature.NativeRootSignature);

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