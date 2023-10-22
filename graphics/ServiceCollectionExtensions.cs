using Microsoft.Extensions.DependencyInjection;
using Vortice.Direct3D12;

namespace graphics;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddEngineCore(this IServiceCollection services) {
        services
            .Configure<GraphicsDebugOptions>(options => { options.Validate = true; })
            .AddSingleton<GraphicsDebugInterface>()
            .AddSingleton<GraphicsDevice>();

        services
            .AddSingleton<DescriptorAllocatorFactory>()
            .AddKeyedSingleton(
                DescriptorHeapType.DepthStencilView,
                DescriptorAllocatorFactory.Factory(1)
            )
            .AddKeyedSingleton(
                DescriptorHeapType.RenderTargetView,
                DescriptorAllocatorFactory.Factory(2)
            )
            .AddKeyedSingleton(
                DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView,
                DescriptorAllocatorFactory.Factory()
            )
            .AddKeyedSingleton(
                DescriptorHeapType.Sampler,
                DescriptorAllocatorFactory.Factory(256)
            )
            .AddKeyedSingleton(
                DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView,
                DescriptorAllocatorFactory.Factory(4096, DescriptorHeapFlags.ShaderVisible)
            )
            .AddKeyedSingleton(
                DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView,
                DescriptorAllocatorFactory.Factory(256, DescriptorHeapFlags.ShaderVisible)
            );

        services
            .AddSingleton<CommandListFactory>()
            .AddSingleton<CommandQueueFactory>()
            .AddKeyedSingleton(CommandListType.Direct, CommandQueueFactory.Factory)
            .AddKeyedSingleton(CommandListType.Compute, CommandQueueFactory.Factory)
            .AddKeyedSingleton(CommandListType.Copy, CommandQueueFactory.Factory);

        return services;
    }
}