using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace window;

public static class XamlHelper {
    [DllImport("Microsoft.ui.xaml.dll")]
    public static extern void XamlCheckProcessRequirements();
}

public class WinUiService<TApplication>(
    Lazy<TApplication> application,
    ILogger<WinUiService<TApplication>> logger,
    IHostApplicationLifetime lifeTime
) : IHostedService where TApplication : Application {

    public TApplication? Application;
    public DispatcherQueue? Queue;
    public DispatcherQueueSynchronizationContext? Context;

    public Task StartAsync(CancellationToken cancellationToken) {
        logger.LogDebug("Starting WinUI");

        var thread = new Thread(Main);
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        logger.LogDebug("Stopping WinUI");

        Queue?.TryEnqueue(Application.Exit);
        Application.UnhandledException -= OnAppOnUnhandledException;

        return Task.CompletedTask;
    }

    private void Main(object? obj) {
        XamlHelper.XamlCheckProcessRequirements();
        WinRT.ComWrappersSupport.InitializeComWrappers();

        try {
            Microsoft.UI.Xaml.Application.Start(StartCallback);
        }
        finally {
            lifeTime.StopApplication();
        }
    }

    void StartCallback(ApplicationInitializationCallbackParams props) {
        Queue = DispatcherQueue.GetForCurrentThread();
        Context = new DispatcherQueueSynchronizationContext(Queue);

        SynchronizationContext.SetSynchronizationContext(Context);

        Application = application.Value;
        Application.UnhandledException += OnAppOnUnhandledException;
    }

    private void OnAppOnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e) {
        e.Handled = false;
        logger.LogCritical(e.Exception, "Unhandled WinUI exception");
    }
}

public static class HostingServiceExtensions {
    public static Lazy<TService> AddLazy<TService>(IServiceProvider provider) =>
        new(() => provider.GetService<TService>()!);
}