using core;
using System.Numerics;

namespace rendering.components;

public record TransformComponent(Matrix4x4 Value) : Component;