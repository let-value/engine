namespace routing;

public record RouteDefinition(
    Type Route,
    Dictionary<string, RouteDefinition>? Routes = null
);

public class RouteOptions {
    public required string InitialRoute { get; set; }
    public required Dictionary<string, RouteDefinition> Routes { get; set; }
}