using System.Numerics;
using Vortice.Mathematics;

namespace shader.SimplePipelineState;

public record struct VertexPositionColor(in Vector3 Position, in Color4 Color) {
    public static readonly unsafe int SizeInBytes = sizeof(VertexPositionColor);
    public readonly Color4 Color = Color;

    public readonly Vector3 Position = Position;
}