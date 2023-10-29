using Microsoft.Extensions.DependencyInjection;
using rendering.loop;

namespace rendering;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddEngineRendering(this IServiceCollection services) {
        services
            .AddTransient<PresenterContext>()
            .AddSingleton<CommandListAllocator>()
            .AddSingleton<IRenderPipeline, NoopRenderPipeline>()
            .AddSingleton<GameLoop>()
            .AddSingleton<FrameRenderer>();

        services.Configure<GameLoopOptions>(options => { options.UpdateRate = 60; });
        services.Configure<RenderBufferingOptions>(options => { options.BufferCount = 2; });

        return services;
    }
}