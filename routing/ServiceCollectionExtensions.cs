using Microsoft.Extensions.DependencyInjection;
using rendering;

namespace routing;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddRouting(this IServiceCollection services) {
        services
            .AddSingleton<IRenderGraph, RoutingRenderGraph>();

        return services;
    }
}