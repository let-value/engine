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

    private readonly CommandList BeginCommandList;
    private readonly CommandList EndCommandList;
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

        BeginCommandList = CommandListFactory.Create(CommandListType.Direct);
        BeginCommandList.Close();

        EndCommandList = CommandListFactory.Create(CommandListType.Direct);
        EndCommandList.Close();

        bufferingOptionsMonitor.OnChange(OnBufferingSizeChanged);
        OnBufferingSizeChanged(bufferingOptionsMonitor.CurrentValue);
    }


    private void OnBufferingSizeChanged(RenderBufferingOptions bufferingOptions) {
        int newBufferCount = bufferingOptions.BufferCount;

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
        BeginCommandList.Reset(SimplePipelineState.PipelineState);

        BeginCommandList.NativeCommandList.ResourceBarrierTransition(
            renderTargetView.Resource.NativeResource,
            ResourceStates.Present,
            ResourceStates.RenderTarget
        );

        BeginCommandList.NativeCommandList.OMSetRenderTargets(
            renderTargetView.CpuDescriptor,
            depthStencilView.CpuDescriptor
        );
        BeginCommandList.NativeCommandList.ClearRenderTargetView(
            renderTargetView.CpuDescriptor,
            Colors.CornflowerBlue
        );
        BeginCommandList.NativeCommandList.ClearDepthStencilView(
            depthStencilView.CpuDescriptor,
            ClearFlags.Depth | ClearFlags.Stencil,
            1.0f,
            0
        );


        BeginCommandList.Close();

        Parallel.For(0, NumThreads, threadIndex => {
            var commandList = CommandListsPerThread[backBufferIndex][threadIndex];

            commandList.Reset(SimplePipelineState.PipelineState);
            commandList.NativeCommandList.OMSetRenderTargets(
                renderTargetView.CpuDescriptor,
                depthStencilView.CpuDescriptor
            );

            commandList.Close();
        });

        EndCommandList.Reset(SimplePipelineState.PipelineState);

        EndCommandList.NativeCommandList.ResourceBarrierTransition(
            renderTargetView.Resource.NativeResource,
            ResourceStates.RenderTarget,
            ResourceStates.Present
        );

        CommandQueue.ExecuteSimple(BeginCommandList);

        foreach (var commandListForThread in CommandListsPerThread) {
            CommandQueue.ExecuteSimple(commandListForThread[backBufferIndex]);
        }

        CommandQueue.ExecuteSimple(EndCommandList);
    }
}