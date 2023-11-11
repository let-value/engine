﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using assets;
using rendering;

namespace sample;

public class SponzaHost {
    public static IHostBuilder CreateHost() => SampleHost
        .CreateHost()
        .ConfigureServices((context, services) => {
            services
                .AddAssetLibrary()
                .AddSingleton<IRenderGraph, SponzaRenderGraph>();
        });
}