using System.Numerics;
using Vortice.Mathematics;

namespace window;

public readonly record struct VertexPositionColor(in Vector3 Position, in Color4 Color) {
    public static readonly unsafe int SizeInBytes = sizeof(VertexPositionColor);

    public readonly Vector3 Position = Position;
    public readonly Color4 Color = Color;
}