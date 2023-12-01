using assets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using rendering;
using scene;

namespace sample;

public class SponzaHost {
    public static IHostBuilder CreateHost() => SampleHost
        .CreateHost()
        .ConfigureServices((context, services) => {
            services
                .AddAssetLibrary()
                .AddScenes()
                .AddSingleton<IRenderGraph, SponzaRenderGraph>();
        });
}