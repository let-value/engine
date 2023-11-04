using Microsoft.Extensions.DependencyInjection;
using rendering.loop;

namespace rendering;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddRendering(this IServiceCollection services) {
        services
            .AddTransient<PresenterContext>()
            .AddTransient<GameLoop>()
            .AddSingleton<CommandListAllocator>()
            .AddSingleton<IRenderGraph, NoopRenderGraph>()
            .AddSingleton<FrameRenderer>();

        services.Configure<GameLoopOptions>(options => { options.UpdateRate = 60; });
        services.Configure<RenderBufferingOptions>(options => { options.BufferCount = 2; });

        return services;
    }
}