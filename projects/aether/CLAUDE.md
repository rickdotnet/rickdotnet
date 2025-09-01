# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Aether is a distributed systems library for .NET built on top of NATS. The project is currently in transition from a legacy implementation to a new architecture focused on building distributed systems in an opinionated way.

### Architecture Transition

- **Legacy Code**: The `legacy/` folder contains the current version and serves as reference for messaging, storage, NATS integration, and builder patterns
- **New Implementation**: The `src/` folder contains the reimagined system library (currently minimal)
- **Core Concepts**: Systems are composed of workers (actor implementation), stores, and endpoints

## Project Structure

```
├── src/Aether/           # New Aether implementation (primary development target)
├── legacy/               # Legacy reference implementation
│   ├── Aether/          # Core messaging and storage abstractions
│   ├── Aether.Extensions.Microsoft.Hosting/  # DI container integration
│   └── Aether.Providers.NATS/  # NATS implementation
├── tests/               # Unit tests
└── build/               # Build automation
```

## Development Commands

### Building
- **Solution**: `dotnet build src/Aether.sln`
- **New project**: `dotnet build src/Aether/`
- **Legacy projects**: `dotnet build legacy/Aether/`

### Testing
- **All tests**: `dotnet test tests/Aether.Tests/`
- **Specific test**: `dotnet test tests/Aether.Tests/ --filter "TestName"`

### Build Automation
The project uses a custom build system:
- **Build and Publish**: `dotnet run --project ./build/build.csproj -- publish`
- **Generate Docs**: `dotnet run --project ./build/build.csproj -- generate-docs`

### Package Management
- **Package ID**: `RickDotNet.Aether`
- **Versioning**: Controlled via `build/version.props`

## Key Architecture Patterns

### Legacy System (Reference)
- **Messaging**: Built around `IMessageHub` for sending/receiving messages with handlers
- **Storage**: `IStore` abstraction with key-value operations and typed operations
- **Client Pattern**: `AetherClient` provides unified access to messaging and storage
- **Provider Model**: Pluggable providers (Memory, NATS) for different backends
- **Builder Pattern**: Fluent configuration APIs via `IAetherBuilder`, `IMessagingBuilder`, `IStorageBuilder`

### Core Abstractions
- **Messages**: `IMessage`, `ICommand`, `IEvent`, `IRequest` with `AetherMessage` wrapper
- **Storage**: Generic operations with `Result<T>` return types for error handling
- **Hosting**: Microsoft.Extensions.Hosting integration for DI and lifecycle management

### NATS Integration
- **Hub**: `NatsHub` implements `IMessageHub` for NATS messaging
- **Storage**: `NatsKvStore` and `NatsObjectStore` for NATS JetStream storage
- **Configuration**: `EndpointConfig` with NATS-specific extensions

## Dependencies
- **.NET**: Targets net9.0 and net8.0
- **Base Library**: References external `Base` project (RickDotNet.Base)
- **Testing**: xUnit framework
- **Extensions**: Microsoft.Extensions.Caching.Memory, Microsoft.Extensions.Primitives

## Development Guidelines

### New Implementation Focus
- Primary development should target `src/Aether/` (currently minimal)
- Use `legacy/` folder as reference for messaging, storage, and NATS patterns
- Maintain .NET 9.0 compatibility as primary target

### Documentation Strategy
- Maximize documentation in the repository
- Use dedicated `docs/` folder for comprehensive documentation
- Include README files when appropriate for specific components

### Code Conventions
- **Nullable**: Enabled throughout the project
- **Implicit Usings**: Enabled for cleaner code
- **Result Pattern**: Use `Result<T>` from Base library for error handling
- **Async**: Prefer async/await patterns with CancellationToken support
- To simpify the middleware logic, all communication will be done over nats. For instance, when sending a message to a worker, even though they are in the same process, the publish will send it onto nats and the worker will ultimately receive it after being dispatched from nats.