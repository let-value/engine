using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Vortice.Direct3D12;
using Vortice.Direct3D12.Debug;

namespace graphics;

public class GraphicsDebugInterface {
    public GraphicsDebugInterface(IOptions<GraphicsDebugOptions> options) {
        var validate = options.Value;

        if (validate.Validate && D3D12.D3D12GetDebugInterface(out ID3D12Debug? debug).Success) {
            debug?.EnableDebugLayer();
            debug?.Dispose();
        }

        if (D3D12.D3D12GetDebugInterface(out ID3D12DeviceRemovedExtendedDataSettings1? dredSettings).Success) {
            dredSettings?.SetAutoBreadcrumbsEnablement(DredEnablement.ForcedOn);
            dredSettings?.SetPageFaultEnablement(DredEnablement.ForcedOn);
            dredSettings?.SetBreadcrumbContextEnablement(DredEnablement.ForcedOn);

            dredSettings?.Dispose();
        }
    }
}