using Microsoft.Extensions.DependencyInjection;
using rendering;

namespace scene;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddScenes(this IServiceCollection services) {
        services
            .AddSingleton<SceneManager>()
            .AddSingleton<IRenderGraph, ScenesRenderGraph>();

        return services;
    }
}