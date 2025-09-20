# Facility Management - SAFE Stack Development Guide

## Architecture Overview

This is a **SAFE Stack** application (Saturn/Fable/Azure/F#) with a clear 3-tier architecture:

- **`src/Shared/`**: F# domain models and API contracts shared between client/server
- **`src/Server/`**: Saturn web server with in-memory storage using F# modules
- **`src/Client/`**: Fable React SPA using Elmish MVU pattern with Feliz UI library

The client compiles F# to JavaScript via Fable, server runs on .NET 8, and both share identical domain logic from `Shared.fs`.

## Critical Build System

**Always use `dotnet run` from root** - never use `dotnet build` or `npm` directly. The `Build.fs` script orchestrates everything:

- `dotnet run` → Concurrent server (watch) + client (Vite dev server) at http://localhost:8080
- `dotnet run -- WatchRunTests` → Run tests in watch mode (server console + client at http://localhost:8081)
- `dotnet run -- Bundle` → Production build (server to `deploy/`, client bundled)
- `dotnet run -- Format` → Fantomas code formatting

The build uses **Paket** (not NuGet directly) - modify `paket.dependencies` for packages, then run restoration through build system.

## F# Patterns & Conventions

### Domain Modeling
```fsharp
// Shared.fs - Always define API contracts as records + interfaces
type ITodosApi = { get: unit -> Async<Todo list>; post: Todo -> Async<Todo list> }

// Server.fs - Implement APIs as record values
let todosApi ctx = { get = fun () -> async { ... }; post = fun todo -> async { ... } }
```

### Client State Management (Elmish MVU)
```fsharp
// Always follow: State -> Message -> update -> View -> dispatch loop
type State = { Todos: RemoteData<Todo list>; Input: string }
type Message = SetInput of string | LoadTodos of ApiCall<unit, Todo list>

// Use SAFE.Client's RemoteData pattern for async operations
match state.Todos with
| NotStarted -> // Initial
| Loading (Some data) -> // Loading with cached data  
| Loaded data -> // Success
```

### Server Module Organization
- Use F# modules (not classes) for business logic: `module Storage = ...`
- Keep mutable state minimal and localized (`ResizeArray` for demo data)
- Saturn applications composed with computation expressions: `application { ... }`

## Testing Strategy

**Dual test setup** for shared code validation:
- **Server tests**: Use Expecto, run with `dotnet run` in `tests/Server/`
- **Client tests**: Use Fable.Mocha, compile to JavaScript, run in browser
- **Shared tests**: Same test code runs in both environments using conditional compilation (`#if FABLE_COMPILER`)

Example shared test pattern:
```fsharp
#if FABLE_COMPILER
open Fable.Mocha
#else  
open Expecto
#endif
```

## Development Workflow

1. **Start development**: `dotnet run` (starts both client/server in watch mode)
2. **Add packages**: Edit `paket.dependencies` → restart build
3. **Client changes**: Auto-reload at http://localhost:8080 via Vite HMR
4. **Server changes**: Auto-restart via `dotnet watch`
5. **Run tests**: `dotnet run -- WatchRunTests` in separate terminal
6. **Format code**: `dotnet run -- Format` (uses Fantomas)

## Key Integration Points

- **API Communication**: Client uses `SAFE.Client.Api.makeProxy<ITodosApi>()` to generate HTTP calls from interface definitions
- **Shared Types**: Domain models in `Shared.fs` ensure type safety across client/server boundary  
- **Asset Pipeline**: Client static assets in `src/Client/public/` served by Saturn's `use_static`
- **CSS Framework**: Tailwind CSS configured via PostCSS, classes applied through Feliz properties

## Project Structure Rules

- **Never mix concerns**: Server logic stays in Server/, client logic in Client/, shared types only in Shared/
- **F# project references**: Server → Shared ← Client (no direct Server ↔ Client references)
- **Build orchestration**: Root `Build.fs` handles all compilation, bundling, and deployment steps
- **Test organization**: Mirror `src/` structure in `tests/` with same project reference patterns

The build system handles Fable compilation, Vite bundling, Saturn hosting, and Azure deployment as integrated pipeline controlled from `Build.fs`.