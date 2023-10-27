namespace rendering.components;

//[Component]
public record struct PrimitiveRenderable : IRenderable {
    public Type? RenderPass { get; init; }
}