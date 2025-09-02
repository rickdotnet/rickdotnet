using RickDotNet.Base;
using RickDotNet.Extensions.Base;

namespace Aether;

public class SystemBuilder
{
    private readonly SystemConfig config = new();
    private readonly List<string> buildErrors = [];

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
        
        builder.Build().Resolve(
            onSuccess: success => config.Endpoints.Add(success),
            onError: error => buildErrors.Add(error)
        );

        return this;
    }

    public SystemBuilder AddWorker(Action<WorkerBuilder> configure)
    {
        var builder = new WorkerBuilder();
        configure(builder);
        builder.Build().Resolve(
            onSuccess: success => config.Workers.Add(success),
            onError: error => buildErrors.Add(error)
        );

        return this;
    }

    public SystemBuilder AddStore(Action<StoreBuilder> configure)
    {
        var builder = new StoreBuilder();
        configure(builder);
        builder.Build().Resolve(
            onSuccess: success => config.Stores.Add(success),
            onError: error => buildErrors.Add(error)
        );
        
        return this;
    }

    internal Result<AetherSystem> Build()
    {
        var validationResult = ValidateConfiguration();
        validationResult.OnError(error => buildErrors.Add(error));
            
        // no op for now
                ProcessSubjectRouting();
                
        return buildErrors.Any() 
            ? Result.Error<AetherSystem>($"Build errors: {string.Join("; ", buildErrors)}") 
            : new AetherSystem(config);
    }

    private Result<Unit> ValidateConfiguration()
    {
        // Validate system configuration
        if (string.IsNullOrWhiteSpace(config.SystemName))
            return Result.Error("System name is required. Use .Named() to set the system name.");

        var systemPrefix = config.SystemPrefix;
        if (string.IsNullOrWhiteSpace(systemPrefix))
            systemPrefix = config.SystemName.ToLowerInvariant().Replace(" ", "-");

        if (!SubjectValidator.IsValid(systemPrefix))
            return Result.Error("Invalid system prefix. Must be a valid NATS subject segment.");

        // Validate no duplicate component names
        List<string> allNames = [];
        allNames.AddRange(config.Endpoints.Select(e => e.Name));
        allNames.AddRange(config.Workers.Select(w => w.Name));
        allNames.AddRange(config.Stores.Select(s => s.Name));

        var duplicates = allNames.GroupBy(n => n).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Any())
            return Result.Error($"Duplicate component names found: {string.Join(", ", duplicates)}");

        // Validate endpoint subjects
        var invalidEndpoints = config.Endpoints
            .Where(e => !SubjectValidator.IsValid(e.Subject))
            .Select(e => e.Subject)
            .ToList();

        if (invalidEndpoints.Any())
            return Result.Error($"Invalid endpoints subjects: {string.Join(", ", invalidEndpoints)}");

        var invalidWorkerSubjects = config.Workers
            .Where(w => !SubjectValidator.IsValid(w.ListenToPattern ?? w.Name.ToLowerInvariant().Replace(" ", "-")))
            .Select(w => w.ListenToPattern)
            .ToList();

        if (invalidWorkerSubjects.Any())
            return Result.Error($"Invalid workers subjects: {string.Join(", ", invalidWorkerSubjects)}");

        return Result.Success();
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
