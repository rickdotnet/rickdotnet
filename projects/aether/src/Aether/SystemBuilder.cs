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
        ValidateConfiguration();
        ProcessSubjectRouting();
        
        return new AetherSystem(config);
    }

    private void ValidateConfiguration()
    {
        // Validate system configuration
        if (string.IsNullOrWhiteSpace(config.SystemName))
            throw new InvalidOperationException("System name is required. Use .Named() to set the system name.");
        
        if (string.IsNullOrWhiteSpace(config.SystemPrefix))
            throw new InvalidOperationException("System prefix is required. Use .Prefixed() to set the system prefix.");

        // Validate no duplicate component names
        var allNames = new List<string>();
        allNames.AddRange(config.Endpoints.Select(e => e.Name));
        allNames.AddRange(config.Workers.Select(w => w.Name));
        allNames.AddRange(config.Stores.Select(s => s.Name));

        var duplicates = allNames.GroupBy(n => n).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Any())
            throw new InvalidOperationException($"Duplicate component names found: {string.Join(", ", duplicates)}");

        // Validate subjects with SubjectValidator
        var validator = new SubjectValidator();
        
        foreach (var endpoint in config.Endpoints)
        {
            if (!validator.IsValid(endpoint.Subject))
                throw new InvalidOperationException($"Invalid subject '{endpoint.Subject}' for endpoint '{endpoint.Name}'");
        }

        foreach (var worker in config.Workers)
        {
            var subject = worker.ListenToPattern ?? worker.Name;
            if (!validator.IsValid(subject))
                throw new InvalidOperationException($"Invalid subject '{subject}' for worker '{worker.Name}'");
        }
    }

    private void ProcessSubjectRouting()
    {
        // Apply system prefix to create full routing subjects
        // This will be used during actual NATS integration
        // For now, we're just validating the pattern will work
        
        foreach (var endpoint in config.Endpoints)
        {
            var fullSubject = $"sys.{config.SystemPrefix}.{endpoint.Subject}";
            // TODO: Store processed subjects for NATS integration
        }

        foreach (var worker in config.Workers)
        {
            var subject = worker.ListenToPattern ?? worker.Name;
            var fullSubject = $"sys.{config.SystemPrefix}.{subject}";
            // TODO: Store processed subjects for NATS integration
        }
    }
}