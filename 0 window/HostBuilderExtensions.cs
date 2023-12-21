using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static engine.BaseHostBuilderExtensions;

namespace engine;

public static class HostBuilderExtensions {
    public static IHostBuilder AddWindowServices(this IHostBuilder hostBuilder) => hostBuilder
        .ConfigureServices(services => {
            services
                .AddSingleton<MainWindow>()
                .AddSingleton(LazyServiceFactory<MainWindow>())
                .AddSingleton<App>()
                .AddSingleton(LazyServiceFactory<App>())
                .AddHostedService<WinUiService<App>>();
        });
}