using System.Runtime.InteropServices;
using SharpGen.Runtime;
using Vortice.Dxc;

namespace shader;

public class Utils {
    public static ReadOnlyMemory<byte> CompileBytecode(DxcShaderStage stage, string shaderName, string entryPoint) {
        var assetsPath = Path.Combine(AppContext.BaseDirectory, "Assets");
        var shaderSource = File.ReadAllText(Path.Combine(assetsPath, shaderName));

        using (ShaderIncludeHandler includeHandler = new(assetsPath)) {
            using var results =
                DxcCompiler.Compile(stage, shaderSource, entryPoint, includeHandler: includeHandler);
            if (results.GetStatus().Failure) {
                throw new(results.GetErrors());
            }

            return results.GetObjectBytecodeMemory();
        }
    }
}

public class ShaderIncludeHandler(params string[] includeDirectories) : CallbackBase, IDxcIncludeHandler {
    private readonly Dictionary<string, SourceCodeBlob> _sourceFiles = new();

    public Result LoadSource(string fileName, out IDxcBlob? includeSource) {
        if (fileName.StartsWith("./")) {
            fileName = fileName.Substring(2);
        }

        var includeFile = GetFilePath(fileName);

        if (string.IsNullOrEmpty(includeFile)) {
            includeSource = default;

            return Result.Fail;
        }

        if (!_sourceFiles.TryGetValue(includeFile, out var sourceCodeBlob)) {
            var data = NewMethod(includeFile);

            sourceCodeBlob = new(data);
            _sourceFiles.Add(includeFile, sourceCodeBlob);
        }

        includeSource = sourceCodeBlob.Blob;

        return Result.Ok;
    }

    protected override void DisposeCore(bool disposing) {
        foreach (var pinnedObject in _sourceFiles.Values) {
            pinnedObject?.Dispose();
        }

        _sourceFiles.Clear();
    }

    private static byte[] NewMethod(string includeFile) {
        return File.ReadAllBytes(includeFile);
    }

    private string? GetFilePath(string fileName) {
        for (var i = 0; i < includeDirectories.Length; i++) {
            var filePath = Path.GetFullPath(Path.Combine(includeDirectories[i], fileName));

            if (File.Exists(filePath)) {
                return filePath;
            }
        }

        return null;
    }

    private class SourceCodeBlob : IDisposable {
        private IDxcBlobEncoding? _blob;
        private byte[] _data;
        private GCHandle _dataPointer;

        public SourceCodeBlob(byte[] data) {
            _data = data;

            _dataPointer = GCHandle.Alloc(data, GCHandleType.Pinned);

            _blob = DxcCompiler.Utils.CreateBlob(_dataPointer.AddrOfPinnedObject(), data.Length, Dxc.DXC_CP_UTF8);
        }

        internal IDxcBlob? Blob => _blob;

        public void Dispose() {
            //_blob?.Dispose();
            _blob = null;

            if (_dataPointer.IsAllocated) {
                _dataPointer.Free();
            }

            _dataPointer = default;
        }
    }
}