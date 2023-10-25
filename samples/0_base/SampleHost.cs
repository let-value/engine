using Microsoft.Extensions.Hosting;
using graphics;
using Microsoft.Extensions.DependencyInjection;
using rendering;
using rendering.loop;
using winui;
using Microsoft.UI.Dispatching;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using UnhandledExceptionEventArgs = Microsoft.UI.Xaml.UnhandledExceptionEventArgs;
using System.Diagnostics;

namespace sample;

public class SampleHost {
    public static IHostBuilder CreateHost() {
        return new HostBuilder()
            .ConfigureServices((context, services) => {
                    services
                        .AddEngineCore()
                        .AddEngineRendering();

                    services.AddSingleton<SwapChainPanelPresenterFactory>();

                    services.Configure<GameLoopOptions>(options => { options.UpdateRate = 1; });

                    services
                        .AddSingleton<MainWindow>()
                        .AddSingleton<App>()
                        .AddSingleton<AppLauncher>();
                }
            );
    }
}

public class AppLauncher(IServiceProvider services) {
    [DllImport("Microsoft.ui.xaml.dll")]
    private static extern void XamlCheckProcessRequirements();

    public void Launch() {
        XamlCheckProcessRequirements();
        WinRT.ComWrappersSupport.InitializeComWrappers();

        Application.Start(_ => {
            try {
                var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);

                var app = services.GetRequiredService<App>();
                app.UnhandledException += OnAppOnUnhandledException;
            }
            catch (Exception e) {
                Debug.WriteLine($"Error application start callback: {e}");
            }
        });
    }

    private void OnAppOnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
        e.Handled = false;
        Debug.WriteLine($"Unhandled exception: {e}");
    }
}