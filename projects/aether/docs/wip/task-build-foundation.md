# Task: Build Foundation - System Builder Architecture

> **Status**: Work in Progress - Implementation Plan  
> **Last Updated**: 2025-09-01  
> **Phase**: Foundation Building

## Overview

This document outlines the plan for building the new Aether library foundation, focusing on the developer experience around system composition. The new architecture centers on **Systems** as composable containers for distributed activity, moving away from generic provider abstractions to a NATS-specific, opinionated approach.

## Architecture Vision

### Core Hierarchy
```
System (Root Container)
- Endpoints (Boundary Services)
- Workers (Message Processors) 
- Stores (Key-Value Storage)
```

### Developer Experience Goal
```csharp
var system = AetherSystem.Create(config => 
{
    config.AddEndpoint("api-gateway", endpoint => 
    {
        endpoint.Subject("api.orders.*");
        endpoint.Handler<OrderApiHandler>();
    });
    
    config.AddWorker<OrderProcessor>("order-worker", worker =>
    {
        worker.ListenTo("orders.process");
        worker.WithStore("order-cache");
    });
    
    config.AddStore("order-cache", store => 
    {
        store.UseNatsKv("orders");
        store.WithExpiration(TimeSpan.FromHours(1));
    });
});

await system.StartAsync();
```

## Implementation Plan

### Part 1: Basic Class Scaffolding

Interfaces aren't required for this initial phase, abstractions will take form when required.

**Core Classes to Create:**
- `AetherSystem` - Root system container
- `SystemBuilder` - Fluent configuration API
- `SystemConfig` - Configuration state holder

**Builders:**
- `SystemBuilder` - Root builder
- `EndpointBuilder` - Endpoint configuration
- `WorkerBuilder` - Worker configuration  
- `StoreBuilder` - Store configuration

**Implementation Priority:**
1. Start with minimal placeholder implementations
2. Focus on API shape and developer experience
3. No actual NATS integration yet - just structure

### Part 2: Builder Implementation Strategy

**SystemBuilder Structure:**
```csharp
public class SystemBuilder
{
    private readonly List<EndpointConfig> endpoints = [];
    private readonly List<WorkerConfig> workers = [];
    private readonly List<StoreConfig> stores = [];
    
    public ISystemBuilder AddEndpoint(string name, Action<IEndpointBuilder> configure);
    public ISystemBuilder AddWorker<T>(string name, Action<IWorkerBuilder>? configure = null);
    public ISystemBuilder AddStore(string name, Action<IStoreBuilder> configure);
    
    internal AetherSystem Build();
}
```

**Fluent Builder Pattern:**
- Each builder returns itself for chaining
- Configuration actions provide typed builders
- Internal state accumulated during build process
- Validation occurs at Build() time

### Part 3: Configuration Objects

**EndpointConfig:**
- Subject patterns for NATS routing
- Handler type registration
- Middleware pipeline configuration
- Request/response patterns

**WorkerConfig:**
- Worker information
  - will be used to construct route
- worker type

**StoreConfig:**
- NATS KV bucket configuration

### Part 4: System Lifecycle

**System Startup Sequence:**
1. Validate all configurations
2. Establish NATS connections
3. Initialize stores (KV buckets)
4. Register endpoints and workers
5. Start message subscriptions
6. Signal system ready

**Error Handling:**
- Result<T> patterns from legacy system
- Graceful degradation where possible
- Clear error messages for configuration issues
- Async exception handling

## Implementation Phases

### Phase 1: Scaffolding ✅ COMPLETED
- [x] Create basic class structure
- [x] Implement clean fluent API shape  
- [x] Add system naming and routing conventions
- [x] Create placeholder implementations
- [x] Add SubjectValidator for future validation
- [x] Create comprehensive tests for API
- [x] Update README with routing documentation

#### Phase 1 Summary
Successfully implemented the foundational system builder architecture with:

**Clean Builder API:**
- `AetherSystem.Create()` with fluent configuration
- `SystemBuilder` with `.Named()` and `.Prefixed()` methods
- All component builders use `.Named()` pattern (no constructor parameters)
- `WorkerBuilder` uses `.Handler<T>()` method (moved from generic `AddWorker<T>`)

**Routing Convention:**
- Established `sys.{system-prefix}.{subject}` pattern
- Default subject behavior (component name used if no explicit subject)
- Comprehensive documentation in README

**System Architecture:**
- System-level store management via `AddStore()` 
- Workers access stores through message context/constructor injection
- All communication flows through NATS (even intra-process)
- Simplified worker configuration (removed `.WithStore()` method)

### Phase 2: System Composition (Current)
- [ ] Implement configuration validation with SubjectValidator
- [ ] Add system lifecycle management and NATS connection
- [ ] Create dependency injection integration
- [ ] Implement subject construction with system prefix
- [ ] Add message context and store access patterns
- [ ] Test complete system composition and routing

### Phase 3: NATS Integration
- [ ] Add NATS connection management
- [ ] Implement endpoint message routing
- [ ] Add worker message processing
- [ ] Integrate NATS KV for stores

### Phase 4: Advanced Features
- [ ] Add middleware pipeline support
- [ ] Implement monitoring and observability
- [ ] Add bridge patterns for external systems
- [ ] Performance optimization

## Key Design Decisions

### NATS-Native Approach
- Built specifically for NATS capabilities
- No generic provider abstractions
- Optimize for NATS-specific features
- Bridge pattern for external integrations

### Opinionated Conventions
- Standard subject naming patterns
- Consistent error handling approach
- Predefined worker lifecycle patterns
- Convention-over-configuration where sensible

### Developer Experience Focus
- Minimal boilerplate code
- Clear error messages
- IntelliSense-friendly APIs
- Easy testing patterns

## Success Metrics

### API Design
- [ ] Can define a system in under 20 lines of code
- [ ] Clear separation of concerns
- [ ] Type-safe configuration
- [ ] Intuitive method naming

### Implementation Quality
- [ ] All builders return expected types
- [ ] Configuration validation catches common errors
- [ ] Clean async/await patterns throughout
- [ ] Proper resource disposal

### Integration Testing
- [ ] Can start/stop system cleanly
- [ ] Message routing works as expected
- [ ] Store operations succeed
- [ ] Error conditions handled gracefully

## Next Steps

1. **Create Class Structure**: Implement basic classes and interfaces
2. **Test API Shape**: Create simple composition examples
3. **Validate DX**: Ensure fluent API feels natural
4. **Iterate Design**: Refine based on usage patterns
5. **Add Implementation**: Begin actual NATS integration

This iterative approach ensures we build the right abstractions while maintaining focus on developer experience and NATS-native performance.