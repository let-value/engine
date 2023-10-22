using graphics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using rendering;
using rendering.loop;
using winui.Graphics;

namespace winui;

public partial class App {
    private readonly IHost Host;

    public App() {
        InitializeComponent();

        Host = new HostBuilder()
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
        await Host.StartAsync();

        var mainWindow = Host.Services.GetRequiredService<MainWindow>();

        mainWindow.Closed += async (_, _) => {
            await Host.StopAsync();
            Host.Dispose();
        };

        mainWindow.Activate();
    }
}