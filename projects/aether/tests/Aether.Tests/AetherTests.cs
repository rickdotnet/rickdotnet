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

    [Fact]
    public void SystemValidatesRequiredConfiguration()
    {
        // Missing system name
        Assert.Throws<InvalidOperationException>(() =>
            AetherSystem.Create(system =>
            {
                system.Prefixed("test");
            }));

        // Missing system prefix
        Assert.Throws<InvalidOperationException>(() =>
            AetherSystem.Create(system =>
            {
                system.Named("TestSystem");
            }));
    }

    [Fact]
    public void SystemValidatesDuplicateNames()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            AetherSystem.Create(system =>
            {
                system.Named("TestSystem").Prefixed("test");

                system.AddEndpoint(endpoint =>
                {
                    endpoint.Named("duplicate")
                            .Subject("test.endpoint")
                            .Handler<TestOrderHandler>();
                });

                system.AddWorker(worker =>
                {
                    worker.Named("duplicate") // Same name as endpoint
                          .Handler<TestOrderProcessor>();
                });
            }));

        Assert.Contains("Duplicate component names found: duplicate", exception.Message);
    }

    [Fact]
    public void SystemValidatesSubjects()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            AetherSystem.Create(system =>
            {
                system.Named("TestSystem").Prefixed("test");

                system.AddEndpoint(endpoint =>
                {
                    endpoint.Named("bad-endpoint")
                            .Subject("invalid..subject") // Invalid double dots
                            .Handler<TestOrderHandler>();
                });
            }));

        Assert.Contains("Invalid subject 'invalid..subject'", exception.Message);
    }

    [Fact]
    public void SystemExposesInspectionProperties()
    {
        var system = AetherSystem.Create(system =>
        {
            system.Named("OrderSystem").Prefixed("orders");

            system.AddEndpoint(endpoint =>
            {
                endpoint.Named("api-gateway")
                        .Subject("api.orders.*")
                        .Handler<TestOrderHandler>();
            });

            system.AddWorker(worker =>
            {
                worker.Named("order-processor")
                      .ListenTo("process")
                      .Handler<TestOrderProcessor>();
            });

            system.AddStore(store =>
            {
                store.Named("order-cache")
                     .UseNatsKv("orders");
            });
        });

        Assert.Equal("OrderSystem", system.SystemName);
        Assert.Equal("orders", system.SystemPrefix);
        Assert.Contains("api-gateway", system.EndpointNames);
        Assert.Contains("order-processor", system.WorkerNames);
        Assert.Contains("order-cache", system.StoreNames);
        Assert.Equal("sys.orders.api.orders.*", system.EndpointSubjects["api-gateway"]);
        Assert.Equal("sys.orders.process", system.WorkerSubjects["order-processor"]);
    }
}

internal class TestOrderHandler
{
}

internal class TestOrderProcessor
{
}
