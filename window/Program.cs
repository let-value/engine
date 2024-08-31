using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using window;

var host = new HostBuilder()
    .ConfigureServices(services => services
        .AddLogging(logging => logging.AddConsole().AddDebug())
        .AddSingleton<App>()
        .AddSingleton<MainWindow>()
        .AddSingleton(HostingServiceExtensions.AddLazy<App>)
        .AddSingleton(HostingServiceExtensions.AddLazy<MainWindow>)
        .AddHostedService<WinUiService<App>>())
    .Build();

await host.RunAsync();