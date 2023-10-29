using graphics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using rendering.loop;
using Vortice.Direct3D12;
using Vortice.Mathematics;

namespace rendering;

public class FrameRenderer : IRenderPipeline {
    private readonly CommandQueue CommandQueue;
    private readonly IRenderPipeline RenderPipeline;
    private readonly CommandListRequest CommandListRequest;

    public FrameRenderer(
        [FromKeyedServices(CommandListType.Direct)]
        CommandQueue commandQueue,
        IRenderPipeline renderPipeline
    ) {
        CommandQueue = commandQueue;
        RenderPipeline = renderPipeline;

        CommandListRequest = new CommandListRequest(
            1,
            new CommandListRequest[] {
                new CommandListRequest(1),
                RenderPipeline.GetCommandListCount(),
                new CommandListRequest(1)
            }
        );
    }

    public void Render(FrameContext frameContext) {
        var (backBufferIndex, commandLists, renderTargetView, depthStencilView, _, _) = frameContext;

        var beginCommandList = CommandListRequest.SliceChild(0, commandLists)[0];

        var renderPipelineSpan = CommandListRequest.SliceChild(1, commandLists);

        var endCommandList = CommandListRequest.SliceChild(2, commandLists)[0];

        beginCommandList.Reset();

        beginCommandList.NativeCommandList.ResourceBarrierTransition(
            renderTargetView.Resource.NativeResource,
            ResourceStates.Present,
            ResourceStates.RenderTarget
        );

        beginCommandList.NativeCommandList.ResourceBarrierTransition(
            depthStencilView.Resource.NativeResource,
            ResourceStates.Present,
            ResourceStates.DepthWrite
        );

        beginCommandList.NativeCommandList.OMSetRenderTargets(
            renderTargetView.CpuDescriptor,
            depthStencilView.CpuDescriptor
        );
        beginCommandList.NativeCommandList.ClearRenderTargetView(
            renderTargetView.CpuDescriptor,
            Colors.CornflowerBlue
        );
        beginCommandList.NativeCommandList.ClearDepthStencilView(
            depthStencilView.CpuDescriptor,
            ClearFlags.Depth | ClearFlags.Stencil,
            1.0f,
            0
        );

        beginCommandList.Close();

        RenderPipeline.Render(frameContext with { CommandLists = renderPipelineSpan });

        endCommandList.Reset();

        endCommandList.NativeCommandList.ResourceBarrierTransition(
            renderTargetView.Resource.NativeResource,
            ResourceStates.RenderTarget,
            ResourceStates.Present
        );

        endCommandList.NativeCommandList.ResourceBarrierTransition(
            depthStencilView.Resource.NativeResource,
            ResourceStates.DepthWrite,
            ResourceStates.Present
        );

        endCommandList.NativeCommandList.Close();

        CommandQueue.ExecuteSimple(commandLists);
    }

    public CommandListRequest GetCommandListCount() {
        return CommandListRequest;
    }

    public void Dispose() { }
}