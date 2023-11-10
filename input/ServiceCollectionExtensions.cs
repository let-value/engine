using Microsoft.Extensions.DependencyInjection;

namespace input;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddInput(this IServiceCollection services) {
        return services;
    }
}