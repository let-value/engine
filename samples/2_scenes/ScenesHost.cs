using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using scene;

namespace sample;

public class ScenesHost {
    public static IHostBuilder CreateHost() => TriangleHost
        .CreateHost()
        .ConfigureServices((context, services) => {
            services
                .AddSingleton<MainWindow, ScenesWindow>()
                .AddScoped<TriangleRenderGraph>()
                .AddTransient<TriangleScene>()
                .Configure<ScenesOptions>(options => {
                    options.InitialScene = "main";
                    options.Scenes = new() {
                        {
                            "main", new() {
                                Scene = typeof(TriangleScene),
                            }
                        }
                    };
                })
                .AddScenes();
        });
}