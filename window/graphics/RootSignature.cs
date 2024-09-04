using System;
using Vortice.Direct3D12;

namespace window.graphics;

public class RootSignature(Device device, RootSignatureDescription1 description) : IDisposable {
    public ID3D12RootSignature DXRootSignature = device.DXDevice.CreateRootSignature(description);

    public void Dispose() {
        DXRootSignature.Dispose();
    }
}