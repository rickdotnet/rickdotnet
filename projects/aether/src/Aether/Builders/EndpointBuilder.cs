using RickDotNet.Base;

namespace Aether;

public class EndpointBuilder
{
    private string? name;
    private string? subject;
    private Type? handlerType;

    public EndpointBuilder Named(string name)
    {
        this.name = name;
        return this;
    }

    public EndpointBuilder Subject(string subjectPattern)
    {
        subject = subjectPattern;
        return this;
    }

    public EndpointBuilder Handler<T>() where T : class
    {
        handlerType = typeof(T);
        return this;
    }

    public EndpointBuilder Handler(Type handlerType)
    {
        this.handlerType = handlerType;
        return this;
    }

    internal Result<EndpointConfig> Build()
    {
        // TODO: Validate configuration with SubjectValidator
        // - Name is required
        // - If no subject specified, use name as subject
        // - Handler type is required
        // - Handler type implements expected interface

        if (string.IsNullOrWhiteSpace(name))
            return Result.Error<EndpointConfig>("Name is required for endpoint");
        
        var endpointName = name;
        var endpointSubject = subject ?? endpointName; // Use name as default subject

        if (handlerType == null)
            return Result.Error<EndpointConfig>($"Handler is required for endpoint '{endpointName}'");

        return Result.Success(new EndpointConfig
        {
            Name = endpointName,
            Subject = endpointSubject,
            HandlerType = handlerType
        });
    }
}