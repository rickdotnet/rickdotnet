namespace Aether;

public class EndpointConfig
{
    public required string Name { get; init; }
    public required string Subject { get; init; }
    public required Type HandlerType { get; init; }
}