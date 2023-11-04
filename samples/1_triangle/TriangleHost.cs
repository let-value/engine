using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using rendering;

namespace sample;

public class TriangleHost {
    public static IHostBuilder CreateHost() => SampleHost
        .CreateHost()
        .ConfigureServices((context, services) => { services.AddSingleton<IRenderGraph, TriangleRenderGraph>(); });
}