using graphics;
using Microsoft.Extensions.DependencyInjection;
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

        CommandListRequest = new(
            1,
            new[] {
                new(1),
                RenderPipeline.GetCommandListCount(),
                new(1)
            }
        );
    }

    public void Render(FrameContext frameContext) {
        var (_, _, commandLists, renderTargetView, depthStencilView, _, _) = frameContext;

        var beginCommandList = CommandListRequest.Slice(commandLists, 0, 0)[0];

        var renderPipelineSpan = CommandListRequest.Slice(commandLists, 0, 1);

        var endCommandList = CommandListRequest.Slice(commandLists, 0, 2)[0];

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