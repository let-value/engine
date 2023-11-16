using assets.assets;
using Microsoft.Extensions.DependencyInjection;

namespace assets;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddAssetLibrary(this IServiceCollection services) {
        return services
            .AddSingleton<Assimp.AssimpContext>()
            .AddSingleton<ModelFactory>()
            .AddSingleton<AssetLibrary>();
    }
}