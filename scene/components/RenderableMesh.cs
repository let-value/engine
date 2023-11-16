using assets.resources;

namespace scene.components;

public record RenderableMesh(MeshResource Mesh) : Component, IRenderableComponent;