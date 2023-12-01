using SharpGen.Runtime;
using System.Runtime.InteropServices;
using Vortice.Dxc;

namespace shader;

public class ShaderUtils {
    public static ReadOnlyMemory<byte> CompileBytecode(DxcShaderStage stage, string shaderName, string entryPoint) {
        var assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets");
        var shaderSource = File.ReadAllText(Path.Combine(assetsPath, shaderName));

        using var includeHandler = new ShaderIncludeHandler(assetsPath);
        using var results = DxcCompiler.Compile(
            stage,
            shaderSource,
            entryPoint,
            includeHandler: includeHandler
        );

        if (results.GetStatus().Failure) {
            throw new(results.GetErrors());
        }

        return results.GetObjectBytecodeMemory();
    }
}

public class ShaderIncludeHandler(params string[] includeDirectories) : CallbackBase, IDxcIncludeHandler {
    private readonly Dictionary<string, SourceCodeBlob> SourceFiles = new();

    public Result LoadSource(string fileName, out IDxcBlob? includeSource) {
        if (fileName.StartsWith("./")) {
            fileName = fileName[2..];
        }

        var includeFile = GetFilePath(fileName);

        if (string.IsNullOrEmpty(includeFile)) {
            includeSource = default;

            return Result.Fail;
        }

        if (!SourceFiles.TryGetValue(includeFile, out var sourceCodeBlob)) {
            var data = NewMethod(includeFile);

            sourceCodeBlob = new(data);
            SourceFiles.Add(includeFile, sourceCodeBlob);
        }

        includeSource = sourceCodeBlob.Blob;

        return Result.Ok;
    }

    protected override void DisposeCore(bool disposing) {
        foreach (var pinnedObject in SourceFiles.Values) pinnedObject?.Dispose();

        SourceFiles.Clear();
    }

    private static byte[] NewMethod(string includeFile) {
        return File.ReadAllBytes(includeFile);
    }

    private string? GetFilePath(string fileName) {
        foreach (var d in includeDirectories) {
            var filePath = Path.GetFullPath(Path.Combine(d, fileName));

            if (File.Exists(filePath)) {
                return filePath;
            }
        }

        return null;
    }

    private class SourceCodeBlob : IDisposable {
        public IDxcBlobEncoding? Blob;
        public byte[] Data;
        public GCHandle DataPointer;

        public SourceCodeBlob(byte[] data) {
            Data = data;
            DataPointer = GCHandle.Alloc(data, GCHandleType.Pinned);
            Blob = DxcCompiler.Utils.CreateBlob(DataPointer.AddrOfPinnedObject(), data.Length, Dxc.DXC_CP_UTF8);
        }

        public void Dispose() {
            Blob?.Dispose();

            if (DataPointer.IsAllocated) {
                DataPointer.Free();
            }

            DataPointer = default;
        }
    }
}