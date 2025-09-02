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
        }).ValueOrDefault();

        Assert.NotNull(system);
        await system.StartAsync();
        await system.StopAsync();
    }

    [Fact]
    public void SystemValidatesRequiredConfiguration()
    {
        var result = AetherSystem.Create(_ => { });
        Assert.True(result.NotSuccessful);
    }

    [Fact]
    public void SystemValidatesDuplicateNames()
    {
        var result = AetherSystem.Create(system =>
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
        });
        
        Assert.True(result.NotSuccessful);
    }

    [Fact]
    public void SystemValidatesSubjects()
    {
        var result = AetherSystem.Create(system =>
        {
            system.Named("TestSystem").Prefixed("test");

            system.AddEndpoint(endpoint =>
            {
                endpoint.Named("bad-endpoint")
                    .Subject("invalid..subject") // Invalid double dots
                    .Handler<TestOrderHandler>();
            });
        });
        
        Assert.True(result.NotSuccessful);
    }

    [Fact]
    public void SystemExposesInspectionProperties()
    {
        var system = AetherSystem.Create(system =>
        {
            system.Named("orders");

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
        }).ValueOrDefault();

        Assert.NotNull(system);
        Assert.Equal("orders", system.SystemName);
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
