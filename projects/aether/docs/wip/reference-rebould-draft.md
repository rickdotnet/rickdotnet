# Documentation: Rebuild

> **Last Updated**: 2025-09-01  

## Project Overview

Project Aether represents the foundational rebuild of the Aether distributed systems library, transitioning from the legacy implementation to a modern, NATS-native architecture designed for building scalable, message-driven applications.

## Project Goals

### Primary Objectives
- **Simplify Distributed Systems**: Provide opinionated patterns that reduce complexity in distributed system development
- **NATS-Native Design**: Build directly on NATS infrastructure for optimal performance and reliability  
- **Actor-Model Implementation**: Introduce "workers" as the core processing abstraction
- **Unified API**: Create consistent interfaces for messaging, storage, and system composition

### Design Principles
- **Opinionated Architecture**: Clear patterns and conventions over configuration flexibility
- **DX**: Prioritize developer experience and provide nice to use surface APIs
- **Composability**: Systems can be assembled in multiple ways to model diverse scenarios
- **Type Safety**: Leverage .NET's type system for compile-time correctness
- **Performance**: Optimize for high-throughput, low-latency distributed operations

## Core Concepts

### Workers
- Actor-model implementation serving as the fundamental processing unit
- Encapsulate state and behavior in isolated, message-driven components  
- Support both stateful and stateless processing patterns

### Stores
- Abstracted storage layer supporting multiple backend implementations
- Key-value and object storage patterns
- Built-in caching and persistence strategies

### Endpoints  
- Communication interfaces that define system boundaries
- Support request-response, publish-subscribe, and streaming patterns
- Configurable routing and message transformation

## Current Architecture State

### Legacy Reference (`/legacy/`)
The legacy implementation provides reference patterns for:
- **Messaging**: `IMessageHub`, `AetherClient`, handler registration
- **Storage**: `IStore` with generic operations and Result patterns
- **NATS Integration**: Hub and storage implementations
- **Builder Patterns**: Fluent configuration APIs
- **DI Integration**: Microsoft.Extensions.Hosting support

### New Implementation (`/src/Aether/`)
Currently minimal with placeholder `Aether.cs` class:
```csharp
public class Aether
{
    // new project, who dis?
}
```

## Development Roadmap

TBD

## Technical Considerations

### Dependencies
- **.NET 9.0** primary target with .NET 8.0 support
- **NATS** for messaging and storage backend
  - Running NATS locally is and easy dependency to solve; will provide scripts for docker and binary
- **RickDotNet.Base** for Result patterns and utilities
- **Microsoft.Extensions** for DI and configuration

### Key Decisions
- **Result Pattern**: Error handling through `Result<T>` types
- **Async-First**: All APIs designed for async/await patterns
- **Nullable Reference Types**: Enabled for compile-time null safety
- **Implicit Usings**: Reduced boilerplate in source files

## Real-World Scenarios

### Enterprise Accounting Service
- **Secretary Endpoint**: Receives external requests
- **Processor Workers**: Handle business logic operations  
- **Audit Store**: Maintains transaction history
- **Notification Workers**: Send status updates

### System Communication
- **API Gateway Endpoint**: Routes external traffic
- **Service Workers**: Execute domain operations
- **Event Store**: Maintains event sourcing patterns
- **Integration Workers**: Handle external service communication

## Success Metrics

### Technical Metrics
- **Developer Experience**: Minimal boilerplate code required

---

*This document serves as a living template for project documentation. Update sections as development progresses and architectural decisions are finalized.*
