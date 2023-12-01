using Microsoft.Extensions.DependencyInjection;


namespace scene;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddScenes(this IServiceCollection services) {
        services
            .AddTransient<SceneImporter>();

        return services;
    }
}