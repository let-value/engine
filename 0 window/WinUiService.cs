using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace engine;

public static class XamlHelper {
    [DllImport("Microsoft.ui.xaml.dll")]
    public static extern void XamlCheckProcessRequirements();
}

public class WinUiService<TApplication>(
    Lazy<TApplication> application,
    ILogger<WinUiService<TApplication>> logger,
    IHostApplicationLifetime lifeTime
) : IHostedService where TApplication : Application {
    private DispatcherQueue? queue;

    public Task StartAsync(CancellationToken cancellationToken) {
        logger.LogDebug("Starting WinUI");

        var thread = new Thread(Main);
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        logger.LogDebug("Stopping WinUI");

        application.Value.UnhandledException -= OnAppOnUnhandledException;
        queue?.TryEnqueue(application.Value.Exit);

        return Task.CompletedTask;
    }

    private void Main(object? obj) {
        XamlHelper.XamlCheckProcessRequirements();
        WinRT.ComWrappersSupport.InitializeComWrappers();

        Application.Start(StartCallback);
        lifeTime.StopApplication();
    }

    void StartCallback(ApplicationInitializationCallbackParams _) {
        queue = DispatcherQueue.GetForCurrentThread();
        var context = new DispatcherQueueSynchronizationContext(queue);
        SynchronizationContext.SetSynchronizationContext(context);

        var app = application.Value;
        app.UnhandledException += OnAppOnUnhandledException;
    }

    private void OnAppOnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e) {
        e.Handled = false;
        logger.LogCritical(e.Exception, "Unhandled WinUI exception");
    }
}