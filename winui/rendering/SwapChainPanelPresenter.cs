﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using rendering;
using SharpGen.Runtime;
using Vortice.DXGI;
using ISwapChainPanelNative = Vortice.WinUI.ISwapChainPanelNative;

namespace winui.rendering;

public class SwapChainPanelPresenter : SwapChainPresenter {
    private readonly SwapChainPanel SwapChainPanel;

    public SwapChainPanelPresenter(
        PresenterContext context,
        FrameRenderer frameRenderer,
        PresentationParameters parameters,
        SwapChainPanel swapChainPanel
    ) : base(
        context,
        frameRenderer,
        parameters,
        CreateSwapChain(
            context,
            parameters,
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
        PresenterContext context,
        PresentationParameters parameters,
        SwapChainPanel swapChainPanel
    ) {
        var bufferingOptions = context.BufferingOptionsMonitor.CurrentValue;
        var debugOptions = context.DebugOptions.Value;

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
            context.CommandQueue.NativeQueue,
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
    PresenterContext context,
    FrameRenderer frameRenderer
) {
    public SwapChainPanelPresenter Create(
        PresentationParameters parameters,
        SwapChainPanel swapChainPanel
    ) {
        return new(
            context,
            frameRenderer,
            parameters,
            swapChainPanel
        );
    }
}