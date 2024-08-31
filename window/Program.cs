using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using window;

var host = new HostBuilder()
    .ConfigureServices((services) => 
        services
            .AddLogging(services => services.AddConsole().AddDebug())
            .AddSingleton<MainWindow>()
            .AddSingleton<App>()
            .AddSingleton(WpfServiceExtensions.AddLazy<App>))
    .UseWpf<App>()
    .Build();

await host.RunAsync();