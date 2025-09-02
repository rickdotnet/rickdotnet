using RickDotNet.Base;

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

    internal Result<StoreConfig> Build()
    {
        // TODO: Validate configuration
        // - Name is required
        // - NATS KV bucket name is required
        // - Expiration is optional

        if (string.IsNullOrWhiteSpace(name))
            return Result.Error<StoreConfig>("Name is required for store");
        
        var storeName = name;

        if (string.IsNullOrWhiteSpace(kvBucket))
            return Result.Error<StoreConfig>($"KV bucket is required for store '{storeName}'");

        return Result.Success(new StoreConfig
        {
            Name = storeName,
            KvBucket = kvBucket,
            Expiration = expiration
        });
    }
}