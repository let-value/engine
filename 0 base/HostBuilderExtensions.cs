using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace engine;

public static class BaseHostBuilderExtensions {
    public static IHostBuilder AddBaseServices(this IHostBuilder hostBuilder) => hostBuilder
        .ConfigureServices((context, services) => services
            .AddLogging(logging => logging
                .AddConfiguration(context.Configuration)
                .SetMinimumLevel(LogLevel.Debug)
                .AddConsole()
                .AddDebug()
            )
        );

    public static Func<IServiceProvider, Lazy<T>> LazyServiceFactory<T>() where T : notnull =>
        provider => new(provider.GetRequiredService<T>);
}