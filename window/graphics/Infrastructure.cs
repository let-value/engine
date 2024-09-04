using System;
using System.Collections.Generic;
using Vortice.DXGI;

namespace window.graphics;

public class Infrastructure : IDisposable {
    public readonly IDXGIFactory2 DXGIFactory;
    public Dictionary<string, Adapter> Adapters;
    public Infrastructure() {
        DXGIFactory = DXGI.CreateDXGIFactory2<IDXGIFactory2>(true);
        Adapters = EnumAdapters();
    }

    public Dictionary<string, Adapter> EnumAdapters() {
        var count = 0;
        var result = new Dictionary<string, Adapter>();

        while (DXGIFactory.EnumAdapters1(count++, out var dxgiAdapter) != ResultCode.NotFound) {
            var adapter = new Adapter(dxgiAdapter);
            result.TryAdd(adapter.Id, adapter);
        }

        return result;
    }

    public void Dispose() {
        foreach (var adapter in Adapters.Values) {
            adapter.Dispose();
        }
        DXGIFactory.Dispose();
    }
}

public static class InfrastructureExtensions {
    public static SwapChain CreateSwapChainForComposition(
        this Infrastructure infrastructure,
        CommandQueue queue,
        SwapChainDescription1 swapChainDescription
    ) {
        return new SwapChain(infrastructure.DXGIFactory.CreateSwapChainForComposition(queue.DXCommandQueue, swapChainDescription));
    }
}