# AGENTS.md

## Build/Test Commands
- **Build solution**: `dotnet build`
- **Build release**: `dotnet build --configuration Release`
- **Run web app**: `dotnet run --project src/Vault.Web`
- **Run CLI tool**: `dotnet run --project tools/Vault.Cli`
- **Restore packages**: `dotnet restore`
- **Publish**: `dotnet publish src/Vault.Web -c Release`

## Architecture Patterns
- **Minimal API**: Vault.Web uses ASP.NET Core Minimal APIs with conditional responses
- **Content Negotiation**: Inspect `Accept` header - `text/html` renders Blazor components, `application/json` returns JSON models
- **DataStar Integration**: Use DataStar ([data-star.dev](https://data-star.dev/guide/getting_started)) for client-side interactivity
- **Web CLI**: Terminal interface using WebTUI ([webtui.ironclad.sh](https://webtui.ironclad.sh/start/intro/)) and DataStar

## Solution Structure
- **Vault**: Core library with business logic, JWT utilities, and NATS integration
- **Vault.Web**: Minimal API with DataStar + Blazor components, includes web CLI functionality
- **Vault.Cli**: Traditional command-line tool (planned to mirror web CLI commands)

## Code Style Guidelines
- **C# version**: .NET 9 with nullable reference types and implicit usings enabled
- **Naming**: PascalCase for classes/types, camelCase for parameters/locals
- **Error handling**: Use `Result<T>` pattern from RickDotNet.Base
- **Imports**: Group system imports first, then third-party, then project imports
- **Formatting**: 4-space indentation, consistent with existing code
- **Types**: Prefer strong typing, use records for immutable data
- **Async**: Use async/await consistently for I/O operations
- **Logging**: Use Serilog with structured logging
- **DI**: Register services in Startup.cs using extension methods