using System.ComponentModel;
using Aether;
using Aether.Abstractions.Messaging;
using Microsoft.Extensions.AI;
using Serilog;

namespace Dusty.Shared.Tools;

public class Foreman
{
    public static AetherClient Aether { get; set; } = null!;
    public static AIFunction[] Tools =>
    [
        AIFunctionFactory.Create(CreateService, Functions.CreateService),
    ];

    [Description("Creates a new service with the specified parameters.")]
    public static async Task<FunctionResult> CreateService(
        [Description("Request to create a new service.")]
        CreateServiceRequest request
    )
    {
        Log.Information("Creating service with name: {ServiceName}, {ImageName}", request.ServiceName, request.ImageName);
        await Aether.Messaging.Send(AetherMessage.For("dusty.execute", request));

        await Task.Delay(5000);
        return FunctionResult.Success;
    }

    public static class Functions
    {
        public const string CreateService = "Foreman.CreateService";
    }
}

public record CreateServiceRequest
{
    [Description("The name of the service to create.")]
    public required string ServiceName { get; set; }

    [Description("The Docker image to use for the service.")]
    public required string ImageName { get; set; }

    [Description("An optional command to run in the service container.")]
    public string[] Command { get; set; } = [];

    [Description("Binds to mount in the service container, where the key is the host path and the value is the container path.")]
    public Dictionary<string, string> Binds { get; set; } = new();

    [Description("A dictionary of volumes to mount in the service container, where the key is the host path and the value is the container path. Example: { \"/host/path\": \"/container/path\" }")]
    public VolumeMapping Volumes { get; set; } = new();

    [Description("A list of port mappings for the service, where each mapping specifies a container port and a host port.")]
    public PortMapping[] PortMappings { get; set; } = [];

    [Description("A dictionary of environment variables to set in the service container, where the key is the variable name and the value is the variable value.")]
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new();
}

public record PortMapping
{
    public uint ContainerPort { get; set; }
    public uint HostPort { get; set; }
}

public record VolumeMapping
{
    public string VolumeName { get; set; } = string.Empty;
    public string ContainerPath { get; set; } = string.Empty;
}

public record EnvironmentVariable
{
    public required string Name { get; set; }
    public required string Value { get; set; }
}
