using graphics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using rendering.loop;
using shader.SimplePipelineState;
using Vortice.Direct3D12;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace rendering;

public class RenderScheduler {
    private const int NumThreads = 4;

    private readonly GraphicsDevice Device;
    private readonly CommandQueue CommandQueue;
    private readonly CommandListFactory CommandListFactory;

    private readonly List<CommandList> BeginCommandLists = new();
    private readonly List<CommandList> EndCommandLists = new();
    private readonly List<List<CommandList>> CommandListsPerThread = new();

    private readonly SimplePipelineState SimplePipelineState;

    public RenderScheduler(
        GraphicsDevice device,
        IOptionsMonitor<RenderBufferingOptions> bufferingOptionsMonitor,
        [FromKeyedServices(CommandListType.Direct)]
        CommandQueue commandQueue,
        CommandListFactory commandListFactory
    ) {
        Device = device;
        CommandQueue = commandQueue;
        CommandListFactory = commandListFactory;

        SimplePipelineState = new SimplePipelineState(device);

        bufferingOptionsMonitor.OnChange(OnBufferingSizeChanged);
        OnBufferingSizeChanged(bufferingOptionsMonitor.CurrentValue);
    }


    private void OnBufferingSizeChanged(RenderBufferingOptions bufferingOptions) {
        int newBufferCount = bufferingOptions.BufferCount;

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

        while (CommandListsPerThread.Count < newBufferCount) {
            var commandListsForThread = new List<CommandList>(NumThreads);

            for (int j = 0; j < NumThreads; j++) {
                var newCommandList = CommandListFactory.Create(CommandListType.Direct);
                newCommandList.Close();

                commandListsForThread.Add(newCommandList);
            }

            CommandListsPerThread.Add(commandListsForThread);
        }

        while (CommandListsPerThread.Count > newBufferCount) {
            var lastCommandListsForThread = CommandListsPerThread[^1];
            foreach (var commandList in lastCommandListsForThread) {
                commandList.Dispose();
            }

            CommandListsPerThread.RemoveAt(CommandListsPerThread.Count - 1);
        }
    }

    public void Render(int backBufferIndex, RenderTargetView renderTargetView, DepthStencilView depthStencilView) {
        var beginCommandList = BeginCommandLists[backBufferIndex];
        var endCommandList = EndCommandLists[backBufferIndex];

        beginCommandList.Reset(SimplePipelineState.PipelineState);

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

        Parallel.For(0, NumThreads, threadIndex => {
            var commandList = CommandListsPerThread[backBufferIndex][threadIndex];

            commandList.Reset(SimplePipelineState.PipelineState);
            commandList.NativeCommandList.OMSetRenderTargets(
                renderTargetView.CpuDescriptor,
                depthStencilView.CpuDescriptor
            );

            commandList.Close();
        });

        endCommandList.Reset(SimplePipelineState.PipelineState);

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

        var commandLists = new CommandList[NumThreads + 2];

        commandLists[0] = beginCommandList;

        for (var i = 0; i < NumThreads; i++) {
            commandLists[i + 1] = CommandListsPerThread[backBufferIndex][i];
        }

        commandLists[^1] = endCommandList;

        CommandQueue.ExecuteSimple(commandLists);
    }
}