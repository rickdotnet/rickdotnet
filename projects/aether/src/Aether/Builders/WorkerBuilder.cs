namespace Aether;

public class WorkerBuilder
{
    private string? name;
    private Type? workerType;
    private string? listenToPattern;

    public WorkerBuilder Named(string name)
    {
        this.name = name;
        return this;
    }

    public WorkerBuilder Handler<T>() where T : class
    {
        workerType = typeof(T);
        return this;
    }

    public WorkerBuilder Handler(Type handlerType)
    {
        workerType = handlerType;
        return this;
    }

    public WorkerBuilder ListenTo(string subjectPattern)
    {
        listenToPattern = subjectPattern;
        return this;
    }

    internal WorkerConfig Build()
    {
        // TODO: Validate configuration with SubjectValidator
        // - Name is required
        // - Worker type is required and implements expected interface
        // - If no ListenTo pattern, use name as default

        var workerName = name ?? throw new InvalidOperationException("Name is required for worker");
        var listenPattern = listenToPattern ?? workerName; // Use name as default listen pattern

        return new WorkerConfig
        {
            Name = workerName,
            WorkerType = workerType ?? throw new InvalidOperationException($"Handler is required for worker '{workerName}'"),
            ListenToPattern = listenPattern
        };
    }
}