using Microsoft.Extensions.DependencyInjection;
using rendering.loop;

namespace rendering;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddEngineRendering(this IServiceCollection services) {
        services
            .AddSingleton<GameLoop>()
            .AddSingleton<RenderScheduler>();

        services.Configure<GameLoopOptions>(options => { options.UpdateRate = 60; });
        services.Configure<RenderBufferingOptions>(options => { options.BufferCount = 2; });

        return services;
    }
}