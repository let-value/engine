using assets.resources;
using core;

namespace rendering.components;

public record RenderableMeshComponent(MeshResource Mesh) : Component, IRenderableComponent;