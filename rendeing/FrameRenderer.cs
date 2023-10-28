using graphics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using rendering.loop;
using Vortice.Direct3D12;
using Vortice.Mathematics;

namespace rendering;

public class FrameRenderer {
    private readonly CommandListFactory CommandListFactory;
    private readonly List<CommandList> BeginCommandLists = new(4);
    private readonly List<CommandList> EndCommandLists = new(4);
    private readonly CommandQueue CommandQueue;

    private readonly IRenderPipeline RenderPipeline;

    public FrameRenderer(
        IOptionsMonitor<RenderBufferingOptions> bufferingOptionsMonitor,
        [FromKeyedServices(CommandListType.Direct)]
        CommandQueue commandQueue,
        CommandListFactory commandListFactory,
        IRenderPipeline renderPipeline
    ) {
        CommandQueue = commandQueue;
        CommandListFactory = commandListFactory;
        RenderPipeline = renderPipeline;

        bufferingOptionsMonitor.OnChange(OnBufferingSizeChanged);
        OnBufferingSizeChanged(bufferingOptionsMonitor.CurrentValue);
    }

    private void OnBufferingSizeChanged(RenderBufferingOptions bufferingOptions) {
        var newBufferCount = bufferingOptions.BufferCount;

        while (BeginCommandLists.Count < newBufferCount) {
            var newBeginCommandList = CommandListFactory.Create(CommandListType.Direct);
            newBeginCommandList.Close();

            BeginCommandLists.Add(newBeginCommandList);
        }

        while (BeginCommandLists.Count > newBufferCount) {
            BeginCommandLists[^1].Dispose();
            BeginCommandLists.RemoveAt(BeginCommandLists.Count - 1);
        }

        while (EndCommandLists.Count < newBufferCount) {
            var newEndCommandList = CommandListFactory.Create(CommandListType.Direct);
            newEndCommandList.Close();

            EndCommandLists.Add(newEndCommandList);
        }

        while (EndCommandLists.Count > newBufferCount) {
            EndCommandLists[^1].Dispose();
            EndCommandLists.RemoveAt(EndCommandLists.Count - 1);
        }
    }

    public void Render(FrameContext frameContext) {
        var (backBufferIndex, renderTargetView, depthStencilView, viewport, scissorRect) = frameContext;

        var beginCommandList = BeginCommandLists[backBufferIndex];
        var endCommandList = EndCommandLists[backBufferIndex];

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

        var renderCommandLists = RenderPipeline.Render(frameContext);

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

        var commandLists = new CommandList[renderCommandLists.Length + 2];

        commandLists[0] = beginCommandList;

        for (var i = 0; i < renderCommandLists.Length; i++) {
            commandLists[i + 1] = renderCommandLists[i];
        }

        commandLists[^1] = endCommandList;

        CommandQueue.ExecuteSimple(commandLists);
    }
}