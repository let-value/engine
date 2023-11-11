using Assimp;
using Microsoft.Extensions.DependencyInjection;

namespace assets;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddAssetLibrary(this IServiceCollection services) {
        return services
            .AddSingleton<AssimpContext>()
            .AddSingleton<AssetFactory>()
            .AddSingleton<AssetLibrary>();
    }
}