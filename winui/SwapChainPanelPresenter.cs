using graphics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using rendering;
using rendering.loop;
using SharpGen.Runtime;
using Vortice.Direct3D12;
using Vortice.DXGI;
using ISwapChainPanelNative = Vortice.WinUI.ISwapChainPanelNative;

namespace winui;

public class SwapChainPanelPresenter : SwapChainPresenter {
    private readonly SwapChainPanel SwapChainPanel;

    public SwapChainPanelPresenter(
        GraphicsDevice device,
        [FromKeyedServices(CommandListType.Direct)]
        CommandQueue commandQueue,
        [FromKeyedServices(DescriptorHeapType.RenderTargetView)]
        DescriptorAllocator renderTargetAllocator,
        [FromKeyedServices(DescriptorHeapType.DepthStencilView)]
        DescriptorAllocator depthStencilViewAllocator,
        IOptionsMonitor<RenderBufferingOptions> bufferingOptionsMonitor,
        IOptions<GraphicsDebugOptions> debugOptions,
        FrameRenderer frameRenderer,
        PresentationParameters parameters,
        SwapChainPanel swapChainPanel
    ) : base(
        device,
        commandQueue,
        renderTargetAllocator,
        depthStencilViewAllocator,
        bufferingOptionsMonitor,
        frameRenderer,
        parameters,
        CreateSwapChain(
            commandQueue,
            parameters,
            bufferingOptionsMonitor.CurrentValue,
            debugOptions.Value,
            swapChainPanel
        )
    ) {
        SwapChainPanel = swapChainPanel;
        SwapChainPanel.SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs args) {
        var width = args.NewSize.Width * SwapChainPanel.CompositionScaleX;
        var height = args.NewSize.Height * SwapChainPanel.CompositionScaleY;

        OnResize((int)width, (int)height);
    }

    private static IDXGISwapChain3 CreateSwapChain(
        CommandQueue commandQueue,
        PresentationParameters parameters,
        RenderBufferingOptions bufferingOptions,
        GraphicsDebugOptions debugOptions,
        SwapChainPanel swapChainPanel
    ) {
        var swapChainDescription = new SwapChainDescription1 {
            Width = parameters.BackBufferWidth,
            Height = parameters.BackBufferHeight,
            Stereo = parameters.Stereo,
            SampleDescription = new(1, 0),
            BufferUsage = Usage.RenderTargetOutput,
            BufferCount = bufferingOptions.BufferCount,
            Scaling = Scaling.Stretch,
            SwapEffect = SwapEffect.FlipSequential,
            Format = parameters.BackBufferFormat,
            Flags = SwapChainFlags.None,
            AlphaMode = AlphaMode.Premultiplied
        };

        using var nativePanel = ComObject.As<ISwapChainPanelNative>(swapChainPanel);

        var factory = DXGI.CreateDXGIFactory2<IDXGIFactory2>(debugOptions.Validate);

        using var tempSwapChain = factory.CreateSwapChainForComposition(
            commandQueue.NativeQueue,
            swapChainDescription
        );

        var swapChain = tempSwapChain.QueryInterface<IDXGISwapChain3>();
        nativePanel.SetSwapChain(swapChain);

        swapChain.MatrixTransform = new() {
            M11 = 1.0f / swapChainPanel.CompositionScaleX,
            M22 = 1.0f / swapChainPanel.CompositionScaleY
        };

        return swapChain;
    }
}

public class SwapChainPanelPresenterFactory(
    GraphicsDevice device,
    [FromKeyedServices(CommandListType.Direct)]
    CommandQueue commandQueue,
    [FromKeyedServices(DescriptorHeapType.RenderTargetView)]
    DescriptorAllocator renderTargetViewAllocator,
    [FromKeyedServices(DescriptorHeapType.DepthStencilView)]
    DescriptorAllocator depthStencilViewAllocator,
    IOptionsMonitor<RenderBufferingOptions> bufferingOptionsMonitor,
    IOptions<GraphicsDebugOptions> debugOptions,
    FrameRenderer frameRenderer
) {
    public SwapChainPanelPresenter Create(
        PresentationParameters parameters,
        SwapChainPanel swapChainPanel
    ) {
        return new(
            device,
            commandQueue,
            renderTargetViewAllocator,
            depthStencilViewAllocator,
            bufferingOptionsMonitor,
            debugOptions,
            frameRenderer,
            parameters,
            swapChainPanel
        );
    }
}