using Microsoft.Extensions.DependencyInjection;

namespace ecs;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddEcsCore(this IServiceCollection services) {
        return services.AddSingleton(ImplementationFactory);
    }

    private static JobScheduler.JobScheduler ImplementationFactory(IServiceProvider arg) {
        return new("ECS");
    }
}