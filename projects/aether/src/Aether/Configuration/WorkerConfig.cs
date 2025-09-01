namespace Aether;

public class WorkerConfig
{
    public required string Name { get; init; }
    public required Type WorkerType { get; init; }
    public string? ListenToPattern { get; init; }
}