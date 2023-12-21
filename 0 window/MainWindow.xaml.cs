using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

namespace engine;

public sealed partial class MainWindow : Window {
    private readonly ILogger<MainWindow> logger;

    public MainWindow(ILogger<MainWindow> logger) {
        this.logger = logger;

        InitializeComponent();
        logger.LogDebug("MainWindow initialized");
    }
}
