using assets.assets;
using Microsoft.Extensions.DependencyInjection;

namespace assets;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddAssetLibrary(this IServiceCollection services) {
        return services
            .AddSingleton(AssimpExtensions.Factory)
            .AddSingleton<ModelFactory>()
            .AddSingleton<AssetLibrary>();
    }
}