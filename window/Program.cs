using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using window;
using static window.HostingServiceExtensions;

var host = new HostBuilder()
    .ConfigureServices(services => services
        .AddLogging(logging => logging.AddConsole().AddDebug())
        .AddSingleton<App>()
        .AddSingleton<MainWindow>()
        .AddSingleton(AddLazy<App>)
        .AddSingleton(AddLazy<MainWindow>)
        .AddHostedService<Ui>())
    .Build();

await host.RunAsync();