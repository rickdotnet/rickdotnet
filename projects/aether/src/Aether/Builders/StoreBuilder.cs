namespace Aether;

public class StoreBuilder
{
    private string? name;
    private string? kvBucket;
    private TimeSpan? expiration;

    public StoreBuilder Named(string name)
    {
        this.name = name;
        return this;
    }

    public StoreBuilder UseNatsKv(string bucketName)
    {
        kvBucket = bucketName;
        return this;
    }

    public StoreBuilder WithExpiration(TimeSpan expiration)
    {
        this.expiration = expiration;
        return this;
    }

    internal StoreConfig Build()
    {
        // TODO: Validate configuration
        // - Name is required
        // - NATS KV bucket name is required
        // - Expiration is optional

        var storeName = name ?? throw new InvalidOperationException("Name is required for store");

        return new StoreConfig
        {
            Name = storeName,
            KvBucket = kvBucket ?? throw new InvalidOperationException($"KV bucket is required for store '{storeName}'"),
            Expiration = expiration
        };
    }
}