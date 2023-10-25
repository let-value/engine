using Microsoft.Extensions.DependencyInjection;
using sample;


var host = SampleHost.CreateHost().Build();
await host.StartAsync();

var launcher = host.Services.GetRequiredService<AppLauncher>();
launcher.Launch();