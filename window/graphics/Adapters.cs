using System;
using Vortice.DXGI;

namespace window.graphics;

public class Adapter(IDXGIAdapter1 dxgiAdapter) : IDisposable {
    public IDXGIAdapter1 DXGIAdapter = dxgiAdapter;
    public AdapterDescription1 DXGIDescription = dxgiAdapter.Description1;
    public string Id = dxgiAdapter.Description1.Luid.ToString();
    public string Name = dxgiAdapter.Description1.Description;

    public void Dispose() {
        DXGIAdapter.Dispose();
    }
}
