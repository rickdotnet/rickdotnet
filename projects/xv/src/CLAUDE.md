# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

CrowdX Vault is a secure key-value storage system built with .NET 9 and NATS. It provides JWT-based authentication, role-based access control, and a modern web interface using Blazor Server and HTMX.

## Technology Stack

- **Backend**: .NET 9 with C#, ASP.NET Core
- **Frontend**: Blazor Server components with HTMX integration
- **Styling**: Tailwind CSS v4 
- **Message Broker**: NATS for key-value storage backend
- **Authentication**: JWT tokens with NATS.Jwt and custom AuthHandler
- **Logging**: Serilog

## Solution Structure

The solution contains three main projects:

- **Vault**: Core library with business logic, JWT utilities, and NATS integration
- **Vault.Web**: Blazor Server web application with API endpoints
- **Vault.Cli**: Command-line tool for vault management and token generation

## Build Commands

```bash
# Build the entire solution
dotnet build

# Run the web application
dotnet run --project Vault.Web

# Run the CLI tool
dotnet run --project Vault.Cli
```

## Architecture

### Authentication & Authorization

- Custom JWT-based authentication using `AuthHandler`
- Role-based policies: `vault.read`, `vault.write`, `vault.admin`
- Claims-based authorization with vault-specific permissions
- Signer validation for vault operations

### API Endpoints

All API endpoints are under `/kv`:
- `POST /kv` - Create new vault (requires authentication)
- `DELETE /kv/{vault}` - Delete vault (requires vault.admin)
- `GET /kv/{vault}/{key}` - Get key value (requires vault.read)
- `PUT /kv/{vault}/{key}` - Set key value (requires vault.write)
- `DELETE /kv/{vault}/{key}` - Delete key (requires vault.write)

### NATS Integration

- Uses NATS KeyValue store for persistent storage
- Connection configured through `VaultSettings` with URL, username, and password
- Vault operations are performed through `INatsKVContext`

### Configuration

Settings are loaded from:
1. Environment variables prefixed with `VAULT_`
2. `vaultSettings.json` file
3. For CLI: `~/.vault/vaultSettings.json`

Key settings:
- `NatsUrl`: NATS server URL (default: nats://localhost:4222)
- `NatsUser`: NATS username
- `NatsPass`: NATS password
- `IssuerSeed`: JWT issuer seed for signing tokens

### Frontend Components

- Uses Blazor Server with interactive components
- HTMX integration for dynamic UI updates
- Tailwind CSS for styling with compiled output in `wwwroot/app.css`
  - input from `Styles/tailwind.css`
- Components organized in `Components/` with Layout, Pages, and Htmx subdirectories

## Development Notes

- The project uses nullable reference types and implicit usings
- Serilog is configured with console output and debug level logging
- Custom extensions in `InternalExtensions.cs` provide helper methods
- JWT utilities handle token creation, validation, and claims parsing
- The CLI tool generates account keypairs and admin tokens for testing