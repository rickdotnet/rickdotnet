# Aether

Glue for distributed systems

## Architecture

Aether is a NATS-native distributed systems library for .NET that provides an opinionated approach to building scalable, message-driven applications through **Systems** composed of **Endpoints**, **Workers**, and **Stores**.

### Core Components

- **System**: A composable container for distributed activity with a unique name and routing prefix
- **Endpoints**: Boundary services that handle external message ingestion
- **Workers**: Actor-ish message processors for internal business logic
- **Stores**: Key-value storage abstraction backed by NATS KV buckets

### Message Routing Convention

All messages in Aether follow the routing pattern:

```
sys.{system-prefix}.{subject}
```

**Examples:**
- System prefix: `orders`
- Endpoint subject: `api.create` → Full route: `sys.orders.api.create`
- Worker listen pattern: `process` → Full route: `sys.orders.process`

**Default Behavior:**
- If no explicit subject is provided, the component name is used as the subject
- Endpoint named "gateway" with no subject → subject becomes "gateway"
- Worker named "processor" with no ListenTo → listens to "processor"

### Usage Example

```csharp
var system = AetherSystem.Create(system =>
{
    system.WithName("OrderSystem")
          .WithPrefix("orders");
          
    system.AddEndpoint(endpoint =>
    {
        endpoint.Named("api-gateway")
                .Subject("api.orders.*")
                .Handler<OrderApiHandler>();
    });
    
    system.AddWorker(worker =>
    {
        worker.Named("order-processor")
              .Handler<OrderProcessor>()
              .ListenTo("process")
              .WithStore("order-cache");
    });
    
    system.AddStore(store =>
    {
        store.Named("order-cache")
             .UseNatsKv("orders")
             .WithExpiration(TimeSpan.FromHours(1));
    });
});

await system.StartAsync();
```

This creates a system where:
- API Gateway listens on `sys.orders.api.orders.*`
- Order Processor listens on `sys.orders.process`
- Order Cache uses NATS KV bucket "orders" with 1-hour expiration
