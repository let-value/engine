using graphics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using rendering.loop;
using Vortice.Direct3D12;

namespace rendering;

public record PresenterContext(
    GraphicsDevice Device,
    [FromKeyedServices(CommandListType.Direct)]
    CommandQueue CommandQueue,
    [FromKeyedServices(DescriptorHeapType.RenderTargetView)]
    DescriptorAllocator RenderTargetAllocator,
    [FromKeyedServices(DescriptorHeapType.DepthStencilView)]
    DescriptorAllocator DepthStencilAllocator,
    IOptionsMonitor<RenderBufferingOptions> BufferingOptionsMonitor,
    IOptions<GraphicsDebugOptions> DebugOptions
);