using Microsoft.Extensions.Hosting;
using graphics;
using Microsoft.Extensions.DependencyInjection;
using rendering;
using rendering.loop;
using Microsoft.UI.Dispatching;
using System.Diagnostics;
using System.Runtime.InteropServices;
using input;
using Microsoft.UI.Xaml;
using winui.input;
using winui.rendering;

namespace sample;

public class SampleHost {
    public static IHostBuilder CreateHost() => new HostBuilder()
        .ConfigureServices((context, services) => {
                services
                    .AddEngineCore()
                    .AddSingleton<IGamepadSource, WindowsGamepadSource>()
                    .AddSingleton<WindowsKeyboardSource>()
                    .AddSingleton<IKeyboardSource, WindowsKeyboardSource>(
                        provider => provider.GetRequiredService<WindowsKeyboardSource>())
                    .AddInput()
                    .AddRendering();

                services.AddSingleton<SwapChainPanelPresenterFactory>();

                services.Configure<GameLoopOptions>(options => { options.UpdateRate = 1; });

                services
                    .AddSingleton<MainWindowContext>()
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