global using Ui = window.WinUiService<window.App>;

using System;
using Microsoft.UI.Xaml;

namespace window;

public partial class App : Application {
    private Lazy<MainWindow> lazyMainWindow;
    public MainWindow? MainWindow;

    public App(Lazy<MainWindow> mainWindow) {
        this.lazyMainWindow = mainWindow;
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args) {
        MainWindow = lazyMainWindow.Value;
        MainWindow.Activate();
    }
}
