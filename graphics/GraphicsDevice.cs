using Vortice.Direct3D;
using Vortice.Direct3D12;

namespace graphics;

public class GraphicsDevice(GraphicsDebugInterface debugInterface) : IDisposable {
    public readonly ID3D12Device NativeDevice = D3D12.D3D12CreateDevice<ID3D12Device>(null, FeatureLevel.Level_11_0);

    public void Dispose() {
        NativeDevice.Dispose();
    }
}