# AGENTS.md - Aether Project Guidelines

## Build & Test Commands

### Building
- **Full solution**: `dotnet build src/Aether.sln`
- **New implementation**: `dotnet build src/Aether/`
- **Legacy projects**: `dotnet build legacy/Aether/`

### Testing
- **All tests**: `dotnet test tests/Aether.Tests/`
- **Single test**: `dotnet test tests/Aether.Tests/ --filter "TestName"`
- **Specific test class**: `dotnet test tests/Aether.Tests/ --filter "ClassName"`

## Code Style Guidelines

### Language & Framework
- **Target**: .NET 9.0
- **Nullable**: Enabled throughout (`<Nullable>enable</Nullable>`)
- **Implicit usings**: Enabled (`<ImplicitUsings>enable</ImplicitUsings>`)
- **Testing**: xUnit framework

### Naming Conventions
- **Classes/Types**: PascalCase (e.g., `AetherSystem`, `EndpointConfig`)
- **Interfaces**: PascalCase with 'I' prefix (e.g., `IMessageHub`, `IStore`)
- **Methods/Properties**: PascalCase (e.g., `StartAsync()`, `SystemName`)
- **Parameters/Local variables**: camelCase (e.g., `cancellationToken`, `endpointConfig`)
- **Constants**: PascalCase (e.g., `DefaultTimeout`)
- **Private fields**: camelCase (e.g., `innerHub`)

### Error Handling
- **Result Pattern**: Use `Result<T>` from RickDotNet.Base for operations that may fail
- **Async Operations**: Always include `CancellationToken` parameter
- **Exception Handling**: Catch specific exceptions, avoid bare `catch` blocks
- **Validation**: Use `ArgumentException` for invalid parameters

### Code Structure
- **Imports**: Group using statements (System.*, then external packages, then project namespaces)
- **Async Methods**: Use `async Task` or `async ValueTask` consistently
- **Builder Pattern**: Use fluent APIs for configuration (e.g., `SystemBuilder`)
- **Dependency Injection**: Prefer constructor injection over property injection
- **Disposal**: Implement `IAsyncDisposable` for resources needing cleanup

### Documentation
- **XML Comments**: Use for public APIs (classes, methods, properties)
- **Inline Comments**: Minimal, only for complex business logic
- **README Files**: Include for components when appropriate

### Architecture Patterns
- **Messaging**: All communication via NATS (even local worker-to-worker)
- **Abstractions**: Define interfaces in dedicated abstraction namespaces
- **Providers**: Pluggable implementations (Memory, NATS) for different backends
- **Configuration**: Use builder patterns for complex object setup

### File Organization
- **New Code**: Primary development in `src/Aether/` (minimal current implementation)
- **Legacy Reference**: Use `legacy/` folder as reference for patterns
- **Tests**: Colocate with source in `tests/` directory
- **Documentation**: Store in dedicated `docs/` folder
- **Agent Context**: Use `docs/wip/` folder for agents to add context and dump information during development