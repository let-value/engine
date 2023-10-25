using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace sample;

public partial class App : Application {
    readonly IServiceProvider Services;

    public App(IServiceProvider services) {
        Services = services;
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args) {
        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Activate();
    }
}