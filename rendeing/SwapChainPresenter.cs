using System.Drawing;
using graphics;
using Microsoft.Extensions.Options;
using rendering.loop;
using Vortice.Direct3D12;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace rendering;

public class SwapChainPresenter : IDisposable {
    private readonly PresenterContext Context;
    private PresentationParameters Parameters;
    private int RenderLatency;

    private readonly PresenterSynchronizationsContext SynchronizationContext;

    private readonly IDXGISwapChain3 SwapChain;
    private readonly FrameRenderer FrameRenderer;
    private readonly List<RenderTargetView> RenderTargets = new();
    private readonly List<FrameContext> FrameContexts = new();

    private int BackBufferIndex;
    private DepthStencilView DepthStencilBuffer;

    public SwapChainPresenter(
        PresenterContext context,
        FrameRenderer frameRenderer,
        PresentationParameters parameters,
        IDXGISwapChain3 swapChain
    ) {
        Context = context;
        SynchronizationContext = new(context);
        SwapChain = swapChain;
        FrameRenderer = frameRenderer;
        Parameters = parameters;

        DepthStencilBuffer = CreateDepthStencilBuffer();

        Context.BufferingOptionsMonitor.OnChange(OnBufferingSizeChanged);
        OnBufferingSizeChanged(Context.BufferingOptionsMonitor.CurrentValue);
    }

    public void Dispose() {
        SynchronizationContext.WaitForGpuCompletion();

        SynchronizationContext.Dispose();
        SwapChain.Dispose();

        DepthStencilBuffer.Dispose();
        DisposeRenderTargets();
    }

    public void DisposeRenderTargets() {
        foreach (var renderTarget in RenderTargets) {
            renderTarget.Dispose();
        }

        RenderTargets.Clear();
    }

    private RenderTargetView CreateRenderTarget(int index) {
        var renderTargetTexture = new Texture(SwapChain.GetBuffer<ID3D12Resource>(index));

        var description = new RenderTargetViewDescription {
            ViewDimension = RenderTargetViewDimension.Texture2D,
            Format = Parameters.BackBufferFormat
        };

        return new(
            Context.Device,
            Context.RenderTargetAllocator,
            renderTargetTexture,
            description
        );
    }

    private DepthStencilView CreateDepthStencilBuffer() {
        var clearValue = new ClearValue(Parameters.DepthStencilFormat, 1.0f);

        var depthStencilTexture = Texture.Create2D(
            Context.Device,
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
            Format = Parameters.DepthStencilFormat
        };

        return new(
            Context.Device,
            Context.DepthStencilAllocator,
            depthStencilTexture,
            description
        );
    }

    private void OnBufferingSizeChanged(RenderBufferingOptions bufferingOptions) {
        SynchronizationContext.WaitForGpuCompletion();
        SynchronizationContext.Lock();

        if (RenderLatency == bufferingOptions.BufferCount) {
            return;
        }

        RenderLatency = bufferingOptions.BufferCount;

        UpdateRenderTargets();
        UpdateFrameContexts();

        SynchronizationContext.Release();
    }

    protected void OnResize(int width, int height) {
        SynchronizationContext.WaitForGpuCompletion();
        SynchronizationContext.Lock();

        Parameters = Parameters with {
            BackBufferWidth = width,
            BackBufferHeight = height
        };

        UpdateDepthStencilBuffer();
        UpdateRenderTargets(true);
        UpdateFrameContexts();

        SynchronizationContext.Release();
    }

    private void UpdateRenderTargets(bool fromScratch = false) {
        if (fromScratch) {
            DisposeRenderTargets();
        }

        SwapChain.ResizeBuffers(
            RenderLatency,
            Parameters.BackBufferWidth,
            Parameters.BackBufferHeight,
            Parameters.BackBufferFormat,
            SwapChainFlags.None
        );

        for (var i = RenderTargets.Count; i < RenderLatency; i++) {
            RenderTargets.Add(CreateRenderTarget(i));
        }

        while (RenderTargets.Count > RenderLatency) {
            var last = RenderTargets[^1];
            last.Dispose();
            RenderTargets.RemoveAt(RenderTargets.Count - 1);
        }

        BackBufferIndex = SwapChain.CurrentBackBufferIndex;
    }

    private void UpdateDepthStencilBuffer() {
        DepthStencilBuffer.Dispose();
        DepthStencilBuffer = CreateDepthStencilBuffer();
    }

    private void UpdateFrameContexts() {
        var viewport = new Viewport(Parameters.BackBufferWidth, Parameters.BackBufferHeight);
        var scissorsRect = new Rectangle(0, 0, Parameters.BackBufferWidth, Parameters.BackBufferHeight);

        FrameContexts.Clear();

        for (var i = FrameContexts.Count; i < RenderLatency; i++) {
            FrameContexts.Add(new(
                i,
                RenderTargets[i],
                DepthStencilBuffer,
                viewport,
                scissorsRect
            ));
        }
    }

    public void Present() {
        SynchronizationContext.Lock();

        var frameContext = FrameContexts[BackBufferIndex];

        FrameRenderer.Render(frameContext);

        try {
            var result = SwapChain.Present(Parameters.SyncInterval, PresentFlags.None, Parameters.PresentParameters);
            if (result.Failure) {
                Context.Device.DebugInterface.HandleDeviceLost(Context.Device);

                return;
            }
        }
        catch {
            Context.Device.DebugInterface.HandleDeviceLost(Context.Device);
        }

        SynchronizationContext.WaitForFrameCompletion(RenderLatency);

        BackBufferIndex = SwapChain.CurrentBackBufferIndex;

        SynchronizationContext.Release();
    }
}