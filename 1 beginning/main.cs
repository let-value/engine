using engine;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .AddBaseServices()
    .Build();

await host.RunAsync();
