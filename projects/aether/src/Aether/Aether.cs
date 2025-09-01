namespace Aether;

public class AetherSystem
{
    private readonly SystemConfig config;
    
    internal AetherSystem(SystemConfig config)
    {
        this.config = config;
    }

    // System inspection properties
    public string SystemName => config.SystemName ?? "Unknown";
    public string SystemPrefix => config.SystemPrefix ?? "Unknown";
    public IReadOnlyList<string> EndpointNames => config.Endpoints.Select(e => e.Name).ToList();
    public IReadOnlyList<string> WorkerNames => config.Workers.Select(w => w.Name).ToList();
    public IReadOnlyList<string> StoreNames => config.Stores.Select(s => s.Name).ToList();
    
    public IReadOnlyDictionary<string, string> EndpointSubjects => 
        config.Endpoints.ToDictionary(e => e.Name, e => $"sys.{config.SystemPrefix}.{e.Subject}");
        
    public IReadOnlyDictionary<string, string> WorkerSubjects =>
        config.Workers.ToDictionary(w => w.Name, w => $"sys.{config.SystemPrefix}.{w.ListenToPattern ?? w.Name}");
    
    public static AetherSystem Create(Action<SystemBuilder> configure)
    {
        var builder = new SystemBuilder();
        configure(builder);
        return builder.Build();
    }
    
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🚀 Starting Aether System: {config.SystemName}");
        Console.WriteLine($"📡 System Prefix: {config.SystemPrefix}");
        
        // Log system composition
        LogSystemComposition();
        
        // TODO: Phase 3 - Establish NATS connections
        // TODO: Phase 3 - Initialize stores (KV buckets)  
        // TODO: Phase 3 - Register endpoints and workers
        // TODO: Phase 3 - Start message subscriptions
        
        Console.WriteLine("✅ System started successfully");
        await Task.CompletedTask;
    }
    
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🛑 Stopping Aether System: {config.SystemName}");
        
        // TODO: Phase 3 - Stop accepting new messages
        // TODO: Phase 3 - Complete in-flight operations
        // TODO: Phase 3 - Dispose NATS connections and resources
        
        Console.WriteLine("✅ System stopped successfully");
        await Task.CompletedTask;
    }

    private void LogSystemComposition()
    {
        Console.WriteLine("📊 System Composition:");
        
        if (config.Endpoints.Any())
        {
            Console.WriteLine("  Endpoints:");
            foreach (var endpoint in config.Endpoints)
            {
                var fullSubject = $"sys.{config.SystemPrefix}.{endpoint.Subject}";
                Console.WriteLine($"    • {endpoint.Name} → {fullSubject} ({endpoint.HandlerType.Name})");
            }
        }

        if (config.Workers.Any())
        {
            Console.WriteLine("  Workers:");
            foreach (var worker in config.Workers)
            {
                var subject = worker.ListenToPattern ?? worker.Name;
                var fullSubject = $"sys.{config.SystemPrefix}.{subject}";
                Console.WriteLine($"    • {worker.Name} → {fullSubject} ({worker.WorkerType.Name})");
            }
        }

        if (config.Stores.Any())
        {
            Console.WriteLine("  Stores:");
            foreach (var store in config.Stores)
            {
                var expiration = store.Expiration?.ToString() ?? "No expiration";
                Console.WriteLine($"    • {store.Name} → NATS KV '{store.KvBucket}' ({expiration})");
            }
        }
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
