using System;
using Microsoft.UI.Xaml;

namespace window;

public partial class App : Application {
    private readonly Lazy<MainWindow> MainWindow;

    public App(Lazy<MainWindow> mainWindow) {
        this.MainWindow = mainWindow;
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args) {
        MainWindow.Value.Activate();
    }
}
