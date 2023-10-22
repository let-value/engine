using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using core;

namespace wpf;

public partial class App
{
    private IHost host;
    
    public App()
    {
        host = new HostBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<GraphicsDevice>();
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }
    
    private async void App_OnStartup(object sender, StartupEventArgs e)
    {
        await host.StartAsync();
        
        var mainWindow = host.Services.GetService<MainWindow>();
        mainWindow?.Show();
    }

    private async void App_OnExit(object sender, ExitEventArgs e)
    {
        await host.StopAsync();
        host.Dispose();
    }
}