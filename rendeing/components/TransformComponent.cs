using core;
using System.Numerics;

namespace rendering.components;

public record TransformComponent(Vector3 Scale, Quaternion Rotation, Vector3 Transform) : Component;