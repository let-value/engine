using Microsoft.Extensions.Hosting;
using graphics;
using Microsoft.Extensions.DependencyInjection;
using rendering;
using rendering.loop;
using winui;
using Microsoft.UI.Dispatching;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;

namespace sample;

public class SampleHost {
    public static IHostBuilder CreateHost() => new HostBuilder()
        .ConfigureServices((context, services) => {
                services
                    .AddEngineCore()
                    .AddEngineRendering();

                services.AddSingleton<SwapChainPanelPresenterFactory>();

                services.Configure<GameLoopOptions>(options => { options.UpdateRate = 1; });

                services
                    .AddSingleton<MainWindow>()
                    .AddSingleton<App>()
                    .AddHostedService<WinUiHostedService<App>>();
            }
        );
}

public static class XamlHelper {
    [DllImport("Microsoft.ui.xaml.dll")]
    public static extern void XamlCheckProcessRequirements();
}

public class WinUiHostedService<TApplication>(IServiceProvider services, IHostApplicationLifetime lifeTime)
    : IHostedService, IDisposable
    where TApplication : Application {
    public Task StartAsync(CancellationToken cancellationToken) {
        var thread = new Thread(Main);
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    private void Main(object? obj) {
        XamlHelper.XamlCheckProcessRequirements();
        WinRT.ComWrappersSupport.InitializeComWrappers();

        Application.Start((p) => {
            var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
            SynchronizationContext.SetSynchronizationContext(context);

            var app = services.GetRequiredService<TApplication>();
            app.UnhandledException += OnAppOnUnhandledException;
        });
        lifeTime.StopApplication();
    }

    private void OnAppOnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e) {
        e.Handled = false;
        Debug.WriteLine($"Unhandled exception: {e}");
    }

    public void Dispose() { }
}