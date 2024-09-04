using System;
using Vortice.DXGI;

namespace window.graphics;

public class SwapChain(IDXGISwapChain1 dxgiSwapChain) : IDisposable {
    public IDXGISwapChain1 DXGISwapChain = dxgiSwapChain;

    public void Dispose() {
        DXGISwapChain.Dispose();
    }
}
