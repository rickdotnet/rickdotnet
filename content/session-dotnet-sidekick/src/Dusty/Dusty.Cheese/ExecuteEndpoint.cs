using System.Text;
using System.Text.Json;
using Aether.Abstractions.Messaging;
using Aether.Messaging;
using Serilog;

namespace Dusty.Cheese;

public class ExecuteEndpoint : IHandle
{
    public static readonly EndpointConfig Config = EndpointConfig.For("dusty.execute");
    
    public Task Handle(MessageContext context, CancellationToken cancellationToken)
    {
        try
        {
            var payload = Encoding.UTF8.GetString(context.Data);
            // pretty json
            var pretty = JsonSerializer.Serialize(
                JsonSerializer.Deserialize<object>(payload),
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                }
            );

            if (string.IsNullOrEmpty(pretty))
            {
                Log.Warning("Null or empty message received");
                return Task.CompletedTask;
            }

            Log.Information("Received ({Subject}):\n{Message}", context.Subject, pretty);
        }
        catch
        {
            Log.Error("Failed to deserialize message with subject {Subject}", context.Subject);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
