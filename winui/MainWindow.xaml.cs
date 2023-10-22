using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using rendering;
using rendering.loop;
using Vortice.DXGI;
using winui.Graphics;

namespace winui;

public sealed partial class MainWindow {
    private readonly GameLoop GameLoop;
    private readonly SwapChainPanelPresenterFactory SwapChainPanelPresenterFactory;
    private SwapChainPanelPresenter? Presenter;

    public MainWindow(GameLoop gameLoop, SwapChainPanelPresenterFactory swapChainPanelPresenterFactory) {
        InitializeComponent();

        SwapChainPanel.Loaded += OnActivated;
        SwapChainPanelPresenterFactory = swapChainPanelPresenterFactory;

        GameLoop = gameLoop;
        GameLoop.OnUpdate += deltaTime => { Presenter?.Present(); };

        //CompositionTarget.Rendering += ((sender, o) => { GameLoop.Run(); });
    }

    private void OnActivated(object sender, RoutedEventArgs e) {
        var width = SwapChainPanel.ActualWidth * SwapChainPanel.CompositionScaleX;
        var height = SwapChainPanel.ActualHeight * SwapChainPanel.CompositionScaleY;

        var parameters = new PresentationParameters {
            BackBufferWidth = (int)width,
            BackBufferHeight = (int)height,
            BackBufferFormat = Format.R8G8B8A8_UNorm,
            DepthStencilFormat = Format.D32_Float,
            SyncInterval = 0,
        };

        Presenter = SwapChainPanelPresenterFactory.Create(parameters, SwapChainPanel);

        GameLoop.Start();
    }
}