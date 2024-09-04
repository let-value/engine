using System;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.DXGI;

namespace window.graphics;

public class Device(Adapter? adapter = null) : IDisposable {
    public readonly ID3D12Device DXDevice = D3D12.D3D12CreateDevice<ID3D12Device>(adapter?.DXGIAdapter, FeatureLevel.Level_11_0);

    public void Dispose() {
        DXDevice.Dispose();
    }
}

public static class DeviceExtensions {
    public static ID3D12Fence CreateFence(
        this Device device,
        ulong initialValue = 0,
        FenceFlags flags = FenceFlags.None
    ) {
        return device.DXDevice.CreateFence(initialValue, flags);
    }
}