using engine;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .AddBaseServices()
    .AddWindowServices()
    .Build();

await host.RunAsync();