using System.Runtime.CompilerServices;
using Vortice.Direct3D12;

namespace graphics;

public record struct RootSignature : IDisposable
{
    public RootSignature(GraphicsDevice device, RootSignatureDescription description)
    {
        var nativeDescription = new RootSignatureDescription1(description.Flags);

        if (description.Parameters != null)
        {
            nativeDescription.Parameters = Unsafe.As<RootParameter1[]>(description.Parameters);
        }

        if (description.StaticSamplers != null)
        {
            nativeDescription.StaticSamplers = Unsafe.As<StaticSamplerDescription[]>(description.StaticSamplers);
        }

        NativeRootSignature = device.NativeDevice.CreateRootSignature(nativeDescription);
    }

    public ID3D12RootSignature NativeRootSignature;

    public readonly void Dispose()
    {
        NativeRootSignature.Dispose();
    }
}