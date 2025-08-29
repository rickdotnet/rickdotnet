using Apollo.Abstractions;
using Microsoft.Extensions.Logging;

namespace DasMonitor.Abstractions;

public class DasMonitorEndpoint : IHandle<AddSignalCommand>, IHandle<ClearSignalCommand>
{
    private readonly DasSignalClient dasClient;
    private readonly ILogger<DasMonitorEndpoint> logger;

    public DasMonitorEndpoint(DasSignalClient dasClient, ILogger<DasMonitorEndpoint> logger)
    {
        this.dasClient = dasClient;
        this.logger = logger;
    }

    public async Task Handle(AddSignalCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending signal to {ProductId} - {ZoneId}", command.ProductId, command.ZoneId);
        var result = await dasClient.SendSignalAsync(command, cancellationToken);
        
        if (!result.Success)
            logger.LogError("Error: {ErrorMessage}", result.ErrorMessage);
    }

    public async Task Handle(ClearSignalCommand message, CancellationToken cancellationToken)
    {
        logger.LogInformation("Clearing signal for {ProductId} - {ZoneId}", message.ProductId, message.ZoneId);
        var result = await dasClient.ClearSignalAsync(message, cancellationToken);
        if (!result.Success)
            logger.LogError("Error: {ErrorMessage}", result.ErrorMessage);
    }
}