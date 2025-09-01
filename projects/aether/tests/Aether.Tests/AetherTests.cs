namespace Aether.Tests;

public class AetherSystemTests
{
    [Fact]
    public void CanCreateSystemWithBuilderApi()
    {
        var system = AetherSystem.Create(system =>
        {
            system.Named("order-system")
                  .Prefixed("orders");

            system.AddEndpoint(endpoint =>
            {
                endpoint.Named("api-gateway")
                        .Subject("api.orders.*")
                        .Handler<TestOrderHandler>();
            });

            system.AddWorker(worker =>
            {
                worker.Named("order-processor")
                      .ListenTo("orders.process")
                      .Handler<TestOrderProcessor>();
            });

            system.AddStore(store =>
            {
                store.Named("order-cache")
                     .UseNatsKv("orders")
                     .WithExpiration(TimeSpan.FromHours(1));
            });
        });

        Assert.NotNull(system);
    }

    [Fact]
    public async Task CanStartAndStopSystem()
    {
        var system = AetherSystem.Create(system =>
        {
            system.Named("TestSystem")
                  .Prefixed("test");

            system.AddStore(store =>
            {
                store.Named("test-store")
                     .UseNatsKv("test");
            });
        });

        await system.StartAsync();
        await system.StopAsync();
    }
}

internal class TestOrderHandler
{
}

internal class TestOrderProcessor
{
}
