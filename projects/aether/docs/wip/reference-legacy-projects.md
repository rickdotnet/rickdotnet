# Legacy Projects Reference

> **Status**: Work in Progress - Reference Documentation  
> **Last Updated**: 2025-09-01  
> **Purpose**: Architectural reference for legacy implementation patterns

## Overview

The legacy Aether implementation was built with a provider abstraction pattern, allowing pluggable messaging and storage backends. This document serves as reference for understanding the architectural evolution leading to the current NATS-specific, opinionated approach.

## Provider Abstraction Architecture

### Core Abstractions

**IMessageHub Interface**
- Central messaging abstraction supporting Send/Request operations
- Handler registration with endpoint configuration
- Generic enough to support Memory and NATS implementations

**IStore Interface**
- Key-value storage abstraction with typed operations
- Support for Get, Upsert, Delete, and ListKeys operations
- Generic Result<T> pattern for error handling

### Provider Implementations

**MemoryHub**
- In-memory message routing using ConcurrentDictionary
- Request-response correlation via TaskCompletionSource
- Useful for testing and development scenarios

**NatsHub** 
- NATS Core and JetStream integration
- Async subscription handling with background tasks
- Header mapping between Aether and NATS message formats

**Storage Providers**
- MemoryStore: Thread-safe in-memory storage
- NatsKvStore/NatsObjectStore: NATS JetStream storage backends

## Builder Pattern System

### Hierarchical Configuration

**IAetherBuilder**
- Root builder providing access to Messaging and Storage builders
- Service registration coordination with DI container

**MessagingBuilder/StorageBuilder**
- Specialized builders for respective concerns
- Hub and Store registration with named instances
- Handler and endpoint registration management

**Builder Implementation**
- Complex service resolution in AetherBuilder.Build()
- Dynamic hub assignment based on configuration
- Handler registration with endpoint providers

### Configuration Patterns

**EndpointConfig**
- Subject-based routing with namespace support
- Configurable delimiters and hub-specific settings
- Extension methods for fluent configuration

**Handler Registration**
- Support for both function handlers and class-based endpoints
- AetherHandler wrapper for message processing
- Integration with dependency injection for endpoint resolution

## Message Processing System

### Message Architecture

**AetherMessage**
- Generic message wrapper with headers and data payload
- Type-aware message creation with ICommand/IEvent/IRequest detection
- Subject routing and request identification patterns

**MessageContext**
- Processing context with message and reply capabilities
- Abstracted reply handling across different providers
- Cancellation token propagation

### Request-Response Pattern

**Correlation Management**
- Request ID generation and tracking
- TaskCompletionSource for async response handling
- Timeout management and error propagation

**Provider-Specific Handling**
- Memory: Direct TaskCompletionSource correlation
- NATS: ReplyTo subject-based correlation

## Client Unification

### AetherClient Pattern

**Unified Interface**
- DefaultMessageHub and DefaultStore wrappers
- Named hub and store resolution
- Static MemoryClient for simple scenarios

**Service Integration**
- Microsoft.Extensions.Hosting integration
- Background service coordination
- Lifecycle management across providers

## Successful Patterns

### Result Pattern
- Consistent error handling with Result<T> types
- Async-first operations with proper cancellation support
- Type safety throughout the messaging pipeline

### Abstraction Benefits
- Clean separation between messaging and storage concerns
- Testability through memory providers
- Consistent API across different backends

### Configuration Flexibility
- Fluent builder APIs for system composition
- Named hub and store instances
- Provider-specific configuration support

## Architectural Context

This generic abstraction approach served as a successful evolution step, providing:
- Clear separation of concerns
- Multiple provider support
- Flexible configuration patterns
- Type-safe operations

The current evolution moves toward a NATS-specific, opinionated approach while preserving the successful patterns identified in this implementation. The new architecture will still abstract where appropriate (such as bridges) while optimizing for NATS-native operations and simplified configuration.