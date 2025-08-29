# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Dusty is a .NET 9 solution with three main projects:
- **Dusty.Cli**: Console application with AI-powered chat interface featuring Dusty Rhodes personality
- **Dusty.Web**: Blazor Server web application with NATS messaging integration
- **Dusty.Shared**: Common library with AI services, tools, and configuration

## Architecture

### AI Integration
- Uses Microsoft.Extensions.AI with OpenAI/GitHub Models integration
- Chat client configured in `Dusty.Shared.Hosting.HostingExtensions.AddAI()`
- Function calling enabled with custom tools (Obsidian, Foreman)
- Dusty Rhodes personality prompts in `Dusty.Shared.Prompts.DustyPrompts`

### Domain-Driven CLI Architecture
- **ChatSession**: Main domain entity managing chat state and processing
- **ChatSessionState**: Immutable state object with message history and mode
- **ChatCommandProcessor**: Handles command parsing and validation
- **StreamingResponseHandler**: Processes AI responses and manages tool call events
- **ChatDisplay**: Handles all Spectre Console rendering and UI concerns

### Configuration System
- `AppConfig` record in Dusty.Shared defines configuration structure
- Environment variables prefixed with `APP_` for CLI, `DUST_` for Web
- Optional `appConfig.json` files for local configuration
- Working directory: `~/.dusty/` with Obsidian vault at `~/.dusty/ObsidianVault`

### Tool System
- **Obsidian Tools**: File operations (save, get, list) for Obsidian vault management
- **Foreman Tools**: Docker service creation with port mapping, volume binding, environment variables
- All tools use Microsoft.Extensions.AI function calling with proper descriptions
- Tool calls are displayed immediately during response processing

### Chat Modes
- **Chat Mode (1)**: Standard chat without tools
- **Tools Mode (2)**: Chat with function calling enabled
- **CompoundBeta Mode (3)**: Uses Groq compound model
- Switch modes with `:m=1`, `:m=2`, `:m=3` commands or `:t`, `:c`, `:comp`

## Common Commands

### Building and Running
```bash
# Build entire solution
dotnet build

# Run CLI application
dotnet run --project Dusty.Cli

# Run Web application
dotnet run --project Dusty.Web

# Build specific project
dotnet build Dusty.Cli/Dusty.Cli.csproj
```

### Development
```bash
# Restore packages
dotnet restore

# Clean build artifacts
dotnet clean

# Run in development mode
dotnet run --project Dusty.Web --environment Development
```

## Key Dependencies

### CLI Project
- Spectre.Console: Rich console UI with markup support
- Markdig: Markdown processing
- Microsoft.Extensions.Hosting: Dependency injection and hosting

### Web Project  
- NATS.Extensions.Microsoft.DependencyInjection: Message bus integration
- RickDotNet.Aether: Custom extensions for hosting and NATS providers

### Shared Project
- Microsoft.Extensions.AI: AI service abstractions
- Microsoft.Extensions.AI.OpenAI: OpenAI provider
- Microsoft.Extensions.AI.Ollama: Ollama provider
- Serilog: Structured logging

## File Structure Notes

- **Domain Layer**: `Dusty.Cli/Domain/` contains ChatSession, ChatSessionState, ChatCommandProcessor
- **Services Layer**: `Dusty.Cli/Services/` contains StreamingResponseHandler, ChatDisplay
- **Demo/Entry Point**: `Dusty.Cli/Demos/CliDemo.cs` orchestrates the chat session
- **Shared Tools**: `Dusty.Shared/Tools/` contains Obsidian and Foreman function implementations
- **UI Utilities**: `MarkdownToSpectreMapper.cs` converts markdown to Spectre Console markup
- **Configuration**: `Dusty.Shared/Hosting/HostingExtensions.cs` sets up dependency injection

## Environment Setup

The application expects:
- .NET 9 SDK
- Optional NATS server for web application messaging
- Obsidian vault directory structure in user's home directory
- GitHub Models API key for AI functionality (configured in hosting extensions)