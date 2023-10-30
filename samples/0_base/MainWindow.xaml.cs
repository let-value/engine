using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using rendering;
using Vortice.DXGI;
using winui;

namespace sample;

public partial class MainWindow : IDisposable {
    private readonly SwapChainPanelPresenterFactory SwapChainPanelPresenterFactory;
    private SwapChainPanelPresenter? Presenter;

    public MainWindow(
        SwapChainPanelPresenterFactory swapChainPanelPresenterFactory,
        IHostApplicationLifetime lifeTime
    ) {
        InitializeComponent();

        SystemBackdrop = new DesktopAcrylicBackdrop();

        SwapChainPanel.Loaded += OnActivated;
        SwapChainPanelPresenterFactory = swapChainPanelPresenterFactory;

        Closed += (_, _) => {
            Dispose();
            lifeTime.StopApplication();
        };
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
        Presenter.RenderLoop.Start();
    }

    public void Dispose() {
        Presenter?.Dispose();
    }
}