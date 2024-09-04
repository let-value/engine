using System;
using Vortice.Direct3D12;

namespace window.graphics;

public class CommandAllocator(Device device, CommandListType type) : IDisposable {
    public CommandListType Type = type;
    public ID3D12CommandAllocator DXCommandAllocator = device.DXDevice.CreateCommandAllocator(type);

    public void Dispose() {
        DXCommandAllocator.Dispose();
    }

    public void Reset() => DXCommandAllocator.Reset();
}
