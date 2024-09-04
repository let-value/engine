using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace window;

public sealed partial class MainWindow : Window {
    private SwapChainPresenter? Presenter;
    private readonly ILogger<MainWindow> logger;

    public MainWindow(ILogger<MainWindow> logger) {
        this.logger = logger;

        InitializeComponent();


        SwapChainPanel.Loaded += OnLoaded;
        CompositionTarget.Rendering += OnRendering;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) {
        logger.LogInformation("SwapChainPanel loaded, creating presenter");
        Presenter = new SwapChainPresenter(SwapChainPanel);
    }

    private void OnRendering(object? sender, object e) {
        Presenter?.Render();
    }
}
