using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpGen.Runtime;
using Vortice.Direct3D12;

namespace window.graphics;

public class CommandQueue(Device device, CommandQueueDescription description) : IDisposable {
    public ID3D12CommandQueue DXCommandQueue = device.DXDevice.CreateCommandQueue(description);

    public void Dispose() {
        DXCommandQueue.Dispose();
    }

    public void ExecuteCommandList(CommandList commandList) =>
        DXCommandQueue.ExecuteCommandList(commandList.DXCommandList);

    public Result Signal(ID3D12Fence fence, ulong currentFence) =>
        DXCommandQueue.Signal(fence, currentFence);
}
