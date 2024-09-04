using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SharpGen.Runtime;
using Vortice;
using Vortice.Direct3D;
using Vortice.Direct3D12;
using Vortice.Dxc;
using Vortice.DXGI;
using Vortice.Mathematics;
using window.graphics;

namespace window;

public class SwapChainPresenter : IDisposable {
    private const int frameCount = 2;
    private Viewport viewport;
    private RawRect scissorRect;
    private DebugInterface debugInterface;
    private Device device;
    private IDXGISwapChain3 swapChain;
    private CommandQueue commandQueue;
    private VertexBufferView vertexBufferView;
    private CommandAllocator commandAllocator;
    private CommandList commandList;
    private RootSignature rootSignature;
    private PipelineState pipelineState;
    private List<RenderTargetView> renderTargetViews = new();
    private int frameIndex;
    private ID3D12Fence fence;
    private ulong fenceValue;
    private AutoResetEvent fenceEvent;

    public SwapChainPresenter(SwapChainPanel swapChainPanel) {

        swapChainPanel.SizeChanged += OnResize;

        var width = (int)(swapChainPanel.ActualWidth * swapChainPanel.CompositionScaleX);
        var height = (int)(swapChainPanel.ActualHeight * swapChainPanel.CompositionScaleY);

        viewport = new Viewport {
            Width = width,
            Height = height,
            MaxDepth = 1.0f,
        };

        scissorRect = new RawRect(0, 0, width, height);

        debugInterface = new DebugInterface();
        var infrastructure = new Infrastructure();
        device = new Device();

        var commandListType = CommandListType.Direct;
        commandQueue = new CommandQueue(device, new() { Type = commandListType });

        using var tempSwapChain = infrastructure.CreateSwapChainForComposition(
            commandQueue,
            new() {
                Width = width,
                Height = height,
                Stereo = false,
                SampleDescription = new(1, 0),
                BufferUsage = Usage.RenderTargetOutput,
                BufferCount = frameCount,
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.FlipSequential,
                Format = Format.R8G8B8A8_UNorm,
                Flags = SwapChainFlags.None,
                AlphaMode = AlphaMode.Premultiplied
            }
        );

        swapChain = tempSwapChain.DXGISwapChain.QueryInterface<IDXGISwapChain3>();

        using var nativePanel = ComObject.As<Vortice.WinUI.ISwapChainPanelNative>(swapChainPanel);
        nativePanel.SetSwapChain(swapChain);

        swapChain.MatrixTransform = new() {
            M11 = 1.0f / swapChainPanel.CompositionScaleX,
            M22 = 1.0f / swapChainPanel.CompositionScaleY
        };

        frameIndex = swapChain.CurrentBackBufferIndex;

        var rtvHeap = new DescriptorHeap(
            device,
            new() {
                DescriptorCount = frameCount,
                Flags = DescriptorHeapFlags.None,
                Type = DescriptorHeapType.RenderTargetView
            });

        for (var n = 0; n < frameCount; n++) {
            var texture = new Texture(swapChain.GetBuffer<ID3D12Resource>(n));

            var view = new RenderTargetView(
                device,
                rtvHeap,
                texture,
                new() {
                    ViewDimension = RenderTargetViewDimension.Texture2D,
                    Format = Format.R8G8B8A8_UNorm
                });

            renderTargetViews.Add(view);
        }

        var rootSignatureFlags = RootSignatureFlags.AllowInputAssemblerInputLayout
                                 | RootSignatureFlags.DenyHullShaderRootAccess
                                 | RootSignatureFlags.DenyDomainShaderRootAccess
                                 | RootSignatureFlags.DenyGeometryShaderRootAccess
                                 | RootSignatureFlags.DenyAmplificationShaderRootAccess
                                 | RootSignatureFlags.DenyMeshShaderRootAccess;

        rootSignature = new RootSignature(device, new() { Flags = rootSignatureFlags });

        var inputLayout = new InputLayoutDescription(
            new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElementDescription("COLOR", 0, Format.R32G32B32A32_Float, 12, 0)
        );

        var vertexShaderByteCode = ShaderUtils.CompileBytecode(DxcShaderStage.Vertex, "Triangle.hlsl", "VSMain");
        var pixelShaderByteCode = ShaderUtils.CompileBytecode(DxcShaderStage.Pixel, "Triangle.hlsl", "PSMain");

        pipelineState = new PipelineState(
            device,
            new() {
                RootSignature = rootSignature.DXRootSignature,
                VertexShader = vertexShaderByteCode,
                PixelShader = pixelShaderByteCode,
                InputLayout = inputLayout,
                SampleMask = uint.MaxValue,
                PrimitiveTopologyType = PrimitiveTopologyType.Triangle,
                RasterizerState = RasterizerDescription.CullCounterClockwise,
                BlendState = BlendDescription.Opaque,
                DepthStencilState = DepthStencilDescription.Default,
                RenderTargetFormats = [Format.R8G8B8A8_UNorm],
                DepthStencilFormat = Format.Unknown,
                SampleDescription = SampleDescription.Default
            });

        var scale = 0.6f;

        var triangleVertices = new[] {
            new VertexPositionColor(new(0f, scale, 0.0f), new(1.0f, 0.0f, 0.0f, 1.0f)),
            new VertexPositionColor(new(scale, -scale, 0.0f), new(0.0f, 1.0f, 0.0f, 1.0f)),
            new VertexPositionColor(new(-scale, -scale, 0.0f), new(0.0f, 0.0f, 1.0f, 1.0f))
        };

        var vertexBufferSize = triangleVertices.Length * VertexPositionColor.SizeInBytes;

        var vertexBuffer = Resource.CreateCommittedResource(device, ResourceDescription.Buffer(vertexBufferSize), HeapType.Upload);
        vertexBuffer.DXResource.SetData(triangleVertices);

        vertexBufferView = new VertexBufferView(
            vertexBuffer.DXResource.GPUVirtualAddress,
            vertexBufferSize,
            VertexPositionColor.SizeInBytes
        );

        commandAllocator = new CommandAllocator(device, commandListType);
        commandList = new CommandList(device, commandAllocator, pipelineState);

        commandList.Close();

        fence = device.CreateFence();
        fenceValue = 1;
        fenceEvent = new AutoResetEvent(false);
    }

    public void Render() {
        commandAllocator.Reset();
        commandList.Reset(commandAllocator, pipelineState);

        commandList.SetGraphicsRootSignature(rootSignature);
        commandList.SetViewport(viewport);
        commandList.SetScissorRect(scissorRect);

        var renderTargetView = renderTargetViews[frameIndex];

        commandList.ResourceBarrierTransition(
            renderTargetView.Resource,
            ResourceStates.Present,
            ResourceStates.RenderTarget
        );

        commandList.SetRenderTargetsView(renderTargetView);
        commandList.ClearRenderTargetView(renderTargetView, new Color4(0, 0.2F, 0.4f, 1));

        commandList.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
        commandList.SetVertexBuffers(0, vertexBufferView);
        commandList.DrawInstanced(3, 1, 0, 0);

        commandList.ResourceBarrierTransition(
            renderTargetView.Resource,
            ResourceStates.RenderTarget,
            ResourceStates.Present
        );

        commandList.Close();

        commandQueue.ExecuteCommandList(commandList);

        var currentFence = fenceValue;

        try {
            if (commandQueue.Signal(fence, currentFence).Failure) {
                throw new InvalidOperationException("Failed to signal fence");
            }

            if (swapChain.Present(1, PresentFlags.None).Failure) {
                throw new InvalidOperationException("Failed to present swap chain");
            }

            

            fenceValue++;
        }
        catch {
            debugInterface.HandleDeviceLost(device);
            throw;
        }

        if (fence.CompletedValue < currentFence) {
            fence.SetEventOnCompletion(currentFence, fenceEvent.SafeWaitHandle.DangerousGetHandle());
            fenceEvent.WaitOne();
        }

        frameIndex = swapChain.CurrentBackBufferIndex;
    }

    private void OnResize(object sender, SizeChangedEventArgs e) {

    }

    public void Dispose() {

    }
}
