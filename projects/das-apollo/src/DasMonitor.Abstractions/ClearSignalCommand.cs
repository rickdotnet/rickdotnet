using Apollo.Abstractions;

namespace DasMonitor.Abstractions;

public record ClearSignalCommand : ICommand
{
    public required string ProductId { get; init; }
    public required string ZoneId { get; init; }
}