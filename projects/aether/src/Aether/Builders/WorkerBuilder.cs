using RickDotNet.Base;

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

    internal Result<WorkerConfig> Build()
    {
        // TODO: Validate configuration with SubjectValidator
        // - Name is required
        // - Worker type is required and implements expected interface
        // - If no ListenTo pattern, use name as default

        if (string.IsNullOrWhiteSpace(name))
            return Result.Error<WorkerConfig>("Name is required for worker");
        
        var workerName = name;
        var listenPattern = listenToPattern ?? workerName; // Use name as default listen pattern

        if (workerType == null)
            return Result.Error<WorkerConfig>($"Handler is required for worker '{workerName}'");

        return Result.Success(new WorkerConfig
        {
            Name = workerName,
            WorkerType = workerType,
            ListenToPattern = listenPattern
        });
    }
}