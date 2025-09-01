namespace Aether;

public class AetherSystem
{
    private readonly SystemConfig config;
    
    internal AetherSystem(SystemConfig config)
    {
        this.config = config;
    }
    
    public static AetherSystem Create(Action<SystemBuilder> configure)
    {
        var builder = new SystemBuilder();
        configure(builder);
        return builder.Build();
    }
    
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement system startup
        // 1. Validate all configurations
        // 2. Establish NATS connections
        // 3. Initialize stores (KV buckets)
        // 4. Register endpoints and workers
        // 5. Start message subscriptions
        // 6. Signal system ready
        await Task.CompletedTask;
    }
    
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement graceful shutdown
        // 1. Stop accepting new messages
        // 2. Complete in-flight operations
        // 3. Dispose resources
        await Task.CompletedTask;
    }
    
    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}

internal class SystemConfig
{
    public string? SystemName { get; set; }
    public string? SystemPrefix { get; set; }
    public List<EndpointConfig> Endpoints { get; } = [];
    public List<WorkerConfig> Workers { get; } = [];
    public List<StoreConfig> Stores { get; } = [];
}
