using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows;
using System.Windows.Threading;

namespace window;
internal class WpfService<TApplication>(
    Lazy<TApplication> application,
    ILogger<WpfService<TApplication>> logger,
    IHostApplicationLifetime lifetime
) : IHostedService where TApplication : Application
{
    private Dispatcher? dispatcher;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Starting WPF application");
        
        var thread = new Thread(Main);
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping WPF application");
        dispatcher?.InvokeShutdown();
        return Task.CompletedTask;
    }

    private void Main(object? obj){
        var app = application.Value;
        dispatcher = app.Dispatcher;
        app.DispatcherUnhandledException += OnAppOnUnhandledException;
        app.Run();
        lifetime.StopApplication();
    }

    private void OnAppOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e){ 
        e.Handled = false;
        logger.LogCritical(e.Exception, "Unhandled exception in WPF application");
    }
}

internal static class WpfServiceExtensions
{
    public static IHostBuilder UseWpf<TApplication>(this IHostBuilder builder) where TApplication : Application =>
        builder.ConfigureServices((services) => services.AddHostedService<WpfService<TApplication>>());

    public static Lazy<TService> AddLazy<TService>(this IServiceProvider provider) =>
        new (() => provider.GetService<TService>());
}