using System.Numerics;
using System.Runtime.CompilerServices;

namespace assets;

public static class AssimpExtensions {
    public static unsafe Matrix4x4 ToNumerics(this Assimp.Matrix4x4 mat) =>
        Unsafe.Read<Matrix4x4>(&mat);

    public static Quaternion ToNumerics(this Assimp.Quaternion quaternion) =>
        new(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);

    public static Vector3 ToNumerics(this Assimp.Vector3D vector) => new(vector.X, vector.Y, vector.Z);

    internal static Assimp.AssimpContext Factory(IServiceProvider _) {
        var context = new Assimp.AssimpContext();
        context.SetConfig(new Assimp.Configs.KeepSceneHierarchyConfig(true));

        return context;
    }
}