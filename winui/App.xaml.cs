using System.Diagnostics;
using graphics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using rendering;
using rendering.loop;
using winui.Graphics;

namespace winui;

public partial class App {
    private readonly IHost host;

    public App() {
        InitializeComponent();

        host = new HostBuilder()
            .ConfigureServices((context, services) => {
                    services
                        .AddEngineCore()
                        .AddEngineRendering();
                    services.AddSingleton<SwapChainPanelPresenterFactory>();
                    services.AddSingleton<MainWindow>();
                    services.Configure<GameLoopOptions>(options => { options.UpdateRate = 1; });
                }
            )
            .Build();
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args) {
        await host.StartAsync();

        var mainWindow = host.Services.GetService<MainWindow>();

        mainWindow.Closed += async (sender, eventArgs) => {
            await host.StopAsync();
            host.Dispose();
        };

        mainWindow.Activate();
    }
}