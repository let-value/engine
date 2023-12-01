using Microsoft.Extensions.Options;
using System.Diagnostics;
using Vortice.Direct3D12;
using Vortice.Direct3D12.Debug;

namespace graphics;

public class GraphicsDebugInterface {
    public GraphicsDebugInterface(IOptions<GraphicsDebugOptions> options) {
        var validate = options.Value;

        if (!validate.Validate) {
            return;
        }

        var debugResult = D3D12.D3D12GetDebugInterface(out ID3D12Debug? debugInterface);

        if (debugInterface == null || debugResult.Failure) {
            throw new InvalidOperationException("Failed to get debug interface");
        }

        debugInterface.EnableDebugLayer();

        var debug = debugInterface.QueryInterface<ID3D12Debug1>();

        debug.EnableDebugLayer();
        debug.SetEnableGPUBasedValidation(true);

        var dredResult = D3D12.D3D12GetDebugInterface(out ID3D12DeviceRemovedExtendedDataSettings1? dredSettings);

        if (dredSettings == null || dredResult.Failure) {
            throw new InvalidOperationException("Failed to get DRED settings");
        }

        dredSettings.SetAutoBreadcrumbsEnablement(DredEnablement.ForcedOn);
        dredSettings.SetPageFaultEnablement(DredEnablement.ForcedOn);
        dredSettings.SetBreadcrumbContextEnablement(DredEnablement.ForcedOn);
    }

    public void HandleDeviceLost(GraphicsDevice device) {
        var removedReason = device.NativeDevice.DeviceRemovedReason;

        Debug.WriteLine($"Device removed! Code: {removedReason.Code} Description: {removedReason.Description}");

        using var dred = device.NativeDevice.QueryInterfaceOrNull<ID3D12DeviceRemovedExtendedData1>();

        if (dred == null) {
            return;
        }

        if (dred.GetAutoBreadcrumbsOutput1(out var dredAutoBreadcrumbsOutput).Success) {
            var currentNode = dredAutoBreadcrumbsOutput.HeadAutoBreadcrumbNode;
            var index = 0;
            while (currentNode != null) {
                var cmdListName = currentNode.CommandListDebugName;
                var cmdQueueName = currentNode.CommandQueueDebugName;
                var expected = currentNode.BreadcrumbCount;
                var actual = currentNode.LastBreadcrumbValue.GetValueOrDefault();

                var errorOccurred = actual > 0 && actual < expected;

                if (actual == 0) {
                    // Don't bother logging nodes that don't submit anything
                    currentNode = currentNode.Next;
                    ++index;
                    continue;
                }

                currentNode = currentNode.Next;
                ++index;
            }
        }

        if (dred.GetPageFaultAllocationOutput1(out var pageFaultOutput).Success) { }
    }
}