using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

namespace engine;

public partial class App : Application {
    private readonly Lazy<MainWindow> MainWindow;
    private readonly ILogger<App> Logger;

    public App(
        Lazy<MainWindow> mainWindow,
        ILogger<App> logger
    ) {
        MainWindow = mainWindow;
        Logger = logger;

        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args) {
        Logger.LogDebug("WinUI application launched");

        MainWindow.Value.Activate();
    }
}