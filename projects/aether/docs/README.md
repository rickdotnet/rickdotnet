# Aether Documentation

## Project Overview

Aether is a distributed systems library for .NET, designed as an opinionated framework for building scalable, message-driven applications on top of NATS. The library aims to provide a unified abstraction layer that simplifies the complexity of distributed system development through well-defined patterns and conventions.

### Core Philosophy

Aether serves as "glue for distributed systems" by providing:
- System: a composable container for distributed acitivty
- **Workers**: Actor-ish implementation for processing messages
- **Stores**: Abstracted key-value storage layer with first-class NATS support  
- **Endpoints**: Boundary services for ingesting external messages

The library enables developers to compose systems in various configurations to model real-world scenarios, from simple request-response patterns to complex enterprise architectures.

## Documentation Structure

This documentation folder contains comprehensive guides, references, and examples for working with Aether:

### Organization

- **`/guides/`** - Step-by-step tutorials and how-to documentation
- **`/reference/`** - API documentation and technical specifications
- **`/examples/`** - Sample projects and implementation patterns
- **`/architecture/`** - System design patterns and best practices
- **`/wip/`** - Work-in-progress drafts and AI-generated documentation

### WIP Folder Purpose

The `wip/` (Work In Progress) folder serves as a staging area for:
- AI-generated documentation drafts
- Experimental documentation approaches
- Community contributions under review
- Documentation templates and scaffolding

Content in the `wip/` folder represents preliminary work that hasn't been formally reviewed and published to the main documentation sections.

## Current State

Aether is currently undergoing a significant architectural transition:
- **Legacy Implementation**: Available in `legacy/` folder for reference
- **New Architecture**: Active development in `src/` focusing on NATS-native design
- **Documentation**: Evolving alongside the rebuild process

## Contributing to Documentation

Documentation contributions follow the same iterative approach as the codebase:
1. Draft content begins in the `wip/` folder
2. Review and refinement process
3. Publication to appropriate documentation sections
4. Continuous updates as the project evolves

---

*This documentation grows alongside the Aether project, reflecting our commitment to maximizing repository-based knowledge sharing.*
