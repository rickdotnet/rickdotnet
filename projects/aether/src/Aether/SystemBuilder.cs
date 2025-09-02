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

        var systemPrefix = config.SystemPrefix;
        if (string.IsNullOrWhiteSpace(systemPrefix))
            systemPrefix = config.SystemName.ToLowerInvariant().Replace(" ", "-");
        
        if (!SubjectValidator.IsValid(systemPrefix))
            throw new InvalidOperationException("Invalid system prefix. Must be a valid NATS subject segment.");

        // Validate no duplicate component names
        List<string> allNames = [];
        allNames.AddRange(config.Endpoints.Select(e => e.Name));
        allNames.AddRange(config.Workers.Select(w => w.Name));
        allNames.AddRange(config.Stores.Select(s => s.Name));

        var duplicates = allNames.GroupBy(n => n).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Any())
            throw new InvalidOperationException($"Duplicate component names found: {string.Join(", ", duplicates)}");

        // Validate endpoint subjects
        var invalidEndpoints = config.Endpoints
            .Where(e => !SubjectValidator.IsValid(e.Subject))
            .Select(e => e.Subject)
            .ToList();
       
        if (invalidEndpoints.Any())
            throw new InvalidOperationException($"Invalid endpoints subjects: {string.Join(", ", invalidEndpoints)}");

        var invalidWorkerSubjects = config.Workers
            .Where(w => !SubjectValidator.IsValid(w.ListenToPattern ?? w.Name.ToLowerInvariant().Replace(" ", "-")))
            .Select(w => w.ListenToPattern)
            .ToList();
        
        if (invalidWorkerSubjects.Any())
            throw new InvalidOperationException($"Invalid workers subjects: {string.Join(", ", invalidWorkerSubjects)}");
        
    }

    private void ProcessSubjectRouting()
    {
        // Apply system prefix to create full routing subjects
        // This will be used during actual NATS integration
        // For now, we're just validating the pattern will work
        
        foreach (var endpoint in config.Endpoints)
        {
            var fullSubject = $"sys.{config.SystemPrefix}.{endpoint.Subject}";
            // TODO: Store processed subjects for publishing to named endpoints
        }

        foreach (var worker in config.Workers)
        {
            var subject = worker.ListenToPattern ?? worker.Name;
            var fullSubject = $"sys.{config.SystemPrefix}.{subject}";
            // TODO: Store processed subjects for NATS integration
        }
    }
}