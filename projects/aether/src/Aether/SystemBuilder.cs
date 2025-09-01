namespace Aether;

public class SystemBuilder
{
    private readonly SystemConfig config = new();

    public SystemBuilder Named(string name)
    {
        config.SystemName = name;
        return this;
    }

    public SystemBuilder Prefixed(string prefix)
    {
        config.SystemPrefix = prefix;
        return this;
    }

    public SystemBuilder AddEndpoint(Action<EndpointBuilder> configure)
    {
        var builder = new EndpointBuilder();
        configure(builder);
        var endpointConfig = builder.Build();
        config.Endpoints.Add(endpointConfig);
        return this;
    }

    public SystemBuilder AddWorker(Action<WorkerBuilder> configure)
    {
        var builder = new WorkerBuilder();
        configure(builder);
        var workerConfig = builder.Build();
        config.Workers.Add(workerConfig);
        return this;
    }

    public SystemBuilder AddStore(Action<StoreBuilder> configure)
    {
        var builder = new StoreBuilder();
        configure(builder);
        var storeConfig = builder.Build();
        config.Stores.Add(storeConfig);
        return this;
    }

    internal AetherSystem Build()
    {
        // TODO: Validate configuration
        // - Ensure system name and prefix are set
        // - Ensure no duplicate names
        // - Validate worker types
        // - Check subject patterns with SubjectValidator
        // - Verify store references
        
        return new AetherSystem(config);
    }
}