using Apollo;
using Apollo.Abstractions;

namespace DasMonitor.Abstractions;

public class DasPublisher
{
    private readonly IPublisher publisher;

    public DasPublisher(ApolloClient apollo)
    {
        publisher = apollo.CreatePublisher(DasMonitorConstants.PublishConfig);
    }

    public Task PublishSignal(AddSignalCommand command)
        => publisher.Send(command);

    public Task ClearSignal(ClearSignalCommand command)
        => publisher.Send(command);
}