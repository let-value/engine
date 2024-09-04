using System;
using Vortice;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.Mathematics;

namespace window.graphics;

public class CommandList(
    Device device,
    CommandAllocator commandAllocator,
    PipelineState? pipelineState = null
) : IDisposable {

    public ID3D12GraphicsCommandList4 DXCommandList = device.DXDevice.CreateCommandList<ID3D12GraphicsCommandList4>(
            commandAllocator.Type,
            commandAllocator.DXCommandAllocator,
            pipelineState?.DXPipelineState
        );

    public void Dispose() {
        DXCommandList.Dispose();
    }

    public void Close() => DXCommandList.Close();

    public void Reset(CommandAllocator commandAllocator, PipelineState pipelineState) =>
        this.DXCommandList.Reset(commandAllocator.DXCommandAllocator, pipelineState.DXPipelineState);

    public void SetGraphicsRootSignature(RootSignature rootSignature) =>
        DXCommandList.SetGraphicsRootSignature(rootSignature.DXRootSignature);

    public void SetViewport(Viewport viewport) =>
        DXCommandList.RSSetViewport(viewport);

    public void SetScissorRect(RawRect scissorRect) =>
        DXCommandList.RSSetScissorRect(scissorRect);

    public void ResourceBarrierTransition(Resource resource, ResourceStates before, ResourceStates after) =>
        DXCommandList.ResourceBarrierTransition(resource.DXResource, before, after);

    public void SetRenderTargetsView(RenderTargetView renderTargetView) =>
        DXCommandList.OMSetRenderTargets(renderTargetView.CpuDescriptor);

    public void ClearRenderTargetView(RenderTargetView renderTargetView, Color4 color) =>
        DXCommandList.ClearRenderTargetView(renderTargetView.CpuDescriptor, color);

    public void SetPrimitiveTopology(PrimitiveTopology topology) =>
        DXCommandList.IASetPrimitiveTopology(topology);

    public void SetVertexBuffers(int slot, VertexBufferView vertexBufferView) =>
        DXCommandList.IASetVertexBuffers(slot, vertexBufferView);

    public void DrawInstanced(int vertexCountPerInstance, int instanceCount, int startVertexLocation, int startInstanceLocation) =>
        DXCommandList.DrawInstanced(vertexCountPerInstance, instanceCount, startVertexLocation, startInstanceLocation);
}
