namespace Aether;

public class StoreConfig
{
    public required string Name { get; init; }
    public required string KvBucket { get; init; }
    public TimeSpan? Expiration { get; init; }
}