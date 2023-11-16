using System.Runtime.CompilerServices;
using assets;
using Assimp;
using graphics;
using rendering;
using scene;
using Vortice.Direct3D12;

namespace sample;

public class SponzaRenderGraph : IRenderGraph {
    private const string Sponza = "sponza";
    private readonly GraphicsDevice Device;
    private readonly Scene Scene;

    public SponzaRenderGraph(GraphicsDevice device, AssetLibrary assetLibrary, SceneImporter sceneImporter) {
        Device = device;

        var asset = assetLibrary.LoadModel(Sponza, "Assets/sponza.obj", PostProcessSteps.GenerateBoundingBoxes);
        Scene = asset.Scene;

        var sceneRoot = sceneImporter.ImportModelAsset(asset, new());
        var sceneGraph = new SceneGraph(sceneRoot);
    }

    public void Dispose() { }

    public CommandListRequest GetCommandListCount() {
        return new(1);
    }

    private ValueTuple<GraphicsResource, GraphicsResource?> UploadMeshToGPU(Mesh mesh) {
        var vertexBufferSize = mesh.VertexCount * Unsafe.SizeOf<Vector3D>();

        var vertexBuffer = new GraphicsResource(Device, ResourceDescription.Buffer(vertexBufferSize), HeapType.Upload);
        vertexBuffer.NativeResource.SetData(mesh.Vertices.ToArray());

        GraphicsResource? indexBuffer = null;
        if (mesh.HasFaces) {
            var indexBufferSize = mesh.Faces.Sum(face => face.IndexCount) * sizeof(int);

            indexBuffer = new(Device, ResourceDescription.Buffer(indexBufferSize), HeapType.Upload);
            indexBuffer.NativeResource.SetData(mesh.Faces.SelectMany(face => face.Indices).ToArray());
        }

        GraphicsResource? textureCoordBuffer = null;
        if (mesh.HasTextureCoords(0)) {
            var textureCoordBufferSize = mesh.TextureCoordinateChannels[0].Count * Unsafe.SizeOf<Vector3D>();
            textureCoordBuffer = new(Device, ResourceDescription.Buffer(textureCoordBufferSize), HeapType.Upload);
            textureCoordBuffer.NativeResource.SetData(mesh.TextureCoordinateChannels[0].ToArray());
        }

        return (vertexBuffer, indexBuffer);
    }

    public void Render(FrameContext frameContext) {
        RenderNode(Scene.RootNode, Matrix4x4.Identity);
    }

    private void RenderNode(Node node, Matrix4x4 parentTransform) {
        var worldTransform = node.Transform * parentTransform;

        foreach (var index in node.MeshIndices) {
            Mesh mesh = Scene.Meshes[index];
            RenderMesh(mesh, worldTransform);
        }

        foreach (var child in node.Children) {
            RenderNode(child, worldTransform);
        }
    }

    private void RenderMesh(Mesh mesh, Matrix4x4 worldTransform) {
        if (mesh.MaterialIndex >= 0) {
            var material = Scene.Materials[mesh.MaterialIndex];
        }
    }
}