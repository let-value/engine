using System.Numerics;

namespace scene.components;

public record TransformComponent(Vector3 Scale, Quaternion Rotation, Vector3 Transform) : Component;