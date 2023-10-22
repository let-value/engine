using System.Diagnostics;
using graphics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using rendering.loop;
using SharpGen.Runtime;
using Vortice.Direct3D12;
using Vortice.DXGI;

namespace rendering;

public class SwapChainPresenter : IDisposable {
    private readonly GraphicsDevice Device;
    private readonly CommandQueue CommandQueue;
    private readonly DescriptorAllocator RendnerTargetAllocator;
    private readonly DescriptorAllocator DepthStencilAllocator;
    private readonly IDXGISwapChain3 SwapChain;
    private readonly RenderScheduler RenderScheduler;
    private PresentationParameters Parameters;

    private int RenderLatency;
    private readonly List<RenderTargetView> RenderTargets = new();
    private DepthStencilView DepthStencilBuffer;

    private readonly SemaphoreSlim ResizeLock = new SemaphoreSlim(1, 1);
    private readonly ID3D12Fence FrameFence;
    private readonly AutoResetEvent FrameFenceEvent;
    private ulong FrameCount;
    private int FrameIndex;
    private int BackBufferIndex;

    public SwapChainPresenter(
        GraphicsDevice device,
        [FromKeyedServices(CommandListType.Direct)]
        CommandQueue commandQueue,
        [FromKeyedServices(DescriptorHeapType.RenderTargetView)]
        DescriptorAllocator renderTargetAllocator,
        [FromKeyedServices(DescriptorHeapType.DepthStencilView)]
        DescriptorAllocator depthStencilViewAllocator,
        IOptionsMonitor<RenderBufferingOptions> bufferingOptionsMonitor,
        RenderScheduler renderScheduler,
        PresentationParameters parameters,
        IDXGISwapChain3 swapChain
    ) {
        Device = device;
        CommandQueue = commandQueue;
        RendnerTargetAllocator = renderTargetAllocator;
        DepthStencilAllocator = depthStencilViewAllocator;
        SwapChain = swapChain;
        RenderScheduler = renderScheduler;
        Parameters = parameters;

        FrameFence = Device.NativeDevice.CreateFence();
        FrameFenceEvent = new(false);

        bufferingOptionsMonitor.OnChange(OnBufferingSizeChanged);
        OnBufferingSizeChanged(bufferingOptionsMonitor.CurrentValue);

        DepthStencilBuffer = CreateDepthStencilBuffer();

        BackBufferIndex = swapChain.CurrentBackBufferIndex;
    }

    private RenderTargetView CreateRenderTarget(int index) {
        var renderTargetTexture = new Texture(SwapChain.GetBuffer<ID3D12Resource>(index));

        var description = new RenderTargetViewDescription {
            ViewDimension = RenderTargetViewDimension.Texture2D,
            Format = Parameters.BackBufferFormat,
        };

        return new RenderTargetView(
            Device,
            RendnerTargetAllocator,
            renderTargetTexture,
            description
        );
    }

    private DepthStencilView CreateDepthStencilBuffer() {
        var clearValue = new ClearValue(Parameters.DepthStencilFormat, 1.0f, 0);

        var depthStencilTexture = Texture.Create2D(
            Device,
            (uint)Parameters.BackBufferWidth,
            (uint)Parameters.BackBufferHeight,
            Parameters.DepthStencilFormat,
            ResourceFlags.AllowDepthStencil | ResourceFlags.DenyShaderResource,
            1,
            Parameters.Stereo ? (ushort)2 : (ushort)1,
            clearValue: clearValue
        );

        var description = new DepthStencilViewDescription {
            ViewDimension = DepthStencilViewDimension.Texture2D,
            Format = Parameters.DepthStencilFormat,
        };

        return new(
            Device,
            DepthStencilAllocator,
            depthStencilTexture,
            description
        );
    }

    private void OnBufferingSizeChanged(RenderBufferingOptions bufferingOptions) {
        WaitForGpuCompletion();

        ResizeLock.Wait();

        RenderLatency = bufferingOptions.BufferCount;

        var bufferCount = bufferingOptions.BufferCount;
        if (bufferCount == RenderTargets.Count) {
            return;
        }

        SwapChain.ResizeBuffers(
            bufferCount,
            Parameters.BackBufferWidth,
            Parameters.BackBufferHeight,
            Parameters.BackBufferFormat,
            SwapChainFlags.None
        );

        for (var i = RenderTargets.Count; i < bufferCount; i++) {
            RenderTargets.Add(CreateRenderTarget(i));
        }

        while (RenderTargets.Count > bufferCount) {
            var last = RenderTargets[^1];
            last.Dispose();
            RenderTargets.RemoveAt(RenderTargets.Count - 1);
        }

        ResizeLock.Release();
    }

    protected void OnResize(int width, int height) {
        WaitForGpuCompletion();

        ResizeLock.Wait();

        Parameters = Parameters with {
            BackBufferWidth = width,
            BackBufferHeight = height,
        };

        DepthStencilBuffer.Dispose();
        DepthStencilBuffer = CreateDepthStencilBuffer();

        foreach (var renderTarget in RenderTargets) {
            renderTarget.Dispose();
        }

        SwapChain.ResizeBuffers(RenderTargets.Count, width, height, Format.Unknown, SwapChainFlags.None);
        BackBufferIndex = SwapChain.CurrentBackBufferIndex;

        for (var i = 0; i < RenderTargets.Count; i++) {
            RenderTargets[i] = CreateRenderTarget(i);
        }


        ResizeLock.Release();
    }

    public void Present() {
        ResizeLock.Wait();

        RenderScheduler.Render(BackBufferIndex, RenderTargets[BackBufferIndex], DepthStencilBuffer);

        try {
            var result = SwapChain.Present(Parameters.SyncInterval, PresentFlags.None, Parameters.PresentParameters);
            if (result.Failure) {
                Device.DebugInterface.HandleDeviceLost(Device);

                return;
            }
        }
        catch {
            Device.DebugInterface.HandleDeviceLost(Device);
        }

        CommandQueue.NativeQueue.Signal(FrameFence, ++FrameCount);

        var gpuFrameCount = FrameFence.CompletedValue;
        var elapsed = (int)(FrameCount - gpuFrameCount);
        if (elapsed >= RenderLatency) {
            FrameFence.SetEventOnCompletion(gpuFrameCount + 1, FrameFenceEvent);
            FrameFenceEvent.WaitOne();
        }

        FrameIndex = (int)(FrameCount % (ulong)RenderLatency);
        BackBufferIndex = SwapChain.CurrentBackBufferIndex;

        ResizeLock.Release();
    }

    private void WaitForGpuCompletion() {
        CommandQueue.NativeQueue.Signal(FrameFence, ++FrameCount);

        if (FrameFence.CompletedValue < FrameCount) {
            FrameFence.SetEventOnCompletion(FrameCount, FrameFenceEvent);
            FrameFenceEvent.WaitOne();
        }
    }

    public void Dispose() {
        WaitForGpuCompletion();

        SwapChain.Dispose();

        FrameFence.Dispose();
        FrameFenceEvent.Dispose();

        DepthStencilBuffer.Dispose();

        foreach (var renderTarget in RenderTargets) {
            renderTarget.Dispose();
        }
    }
}