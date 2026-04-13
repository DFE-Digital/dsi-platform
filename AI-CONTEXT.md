# AI Assistant Context – DSI Platform

> This file helps Claude Code, GitHub Copilot, and other AI assistants understand this codebase quickly. Read this before making suggestions.

---

## What This Is

A .NET 8 rewrite of the DfE Sign-in (DSI) platform (previously Node.js). It uses a Clean Architecture interaction dispatcher pattern throughout — **not a conventional layered MVC/API pattern**.

---

## Critical Patterns — Don't Deviate From These

### 1. Interaction Dispatcher (not direct service injection)

Business logic is invoked exclusively through `IInteractionDispatcher.DispatchAsync<TRequest, TResponse>()`.

**DO NOT** inject use cases or repositories directly into controllers or other use cases. Always dispatch.

```csharp
// Correct
var result = await interaction.DispatchAsync(new ChangeJobTitleRequest(userId, newTitle));

// Wrong — do not do this
var useCase = new ChangeJobTitleUseCase(...);
await useCase.InvokeAsync(...);
```

### 2. Use Case Structure

Every use case must:
- Extend `Interactor<TRequest, TResponse>`
- Call `context.ThrowIfHasValidationErrors()` as the **first line** of `InvokeAsync`
- Use EF via injected `IUnitOfWork*` interfaces, not `DbContext` directly
- Be registered in `InternalApi/Configuration/` via `services.AddInteractor<TRequest, TUseCase>()`

```csharp
public sealed class MyUseCase(IUnitOfWorkDirectories unitOfWork)
    : Interactor<MyRequest, MyResponse>
{
    public override async Task<MyResponse> InvokeAsync(
        InteractionContext<MyRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();
        // EF logic here
        return new MyResponse();
    }
}
```

### 3. Request/Response Types are Records

All contract types in `Core.Contracts` must be `record` (not `class`). This enables shallow equality for test mocking.

```csharp
public record MyRequest(Guid UserId, string Value) : IInteractionRequest;
public record MyResponse(string Result) : IInteractionResponse;
```

Data annotation validation goes on the **Request** type. Validation messages should work for both self-service UI and support console contexts.

### 4. Exceptions

Business exceptions must extend `InteractionException` or `NotFoundInteractionException`.
Use `[Persist]` on exception properties that should cross the API boundary.

```csharp
public sealed class UserNotFoundException : NotFoundInteractionException
{
    [Persist]
    public Guid UserId { get; init; }

    public static UserNotFoundException FromUserId(Guid userId) =>
        new() { UserId = userId };
}
```

Never throw `InvalidOperationException`, `ArgumentException`, etc. as business errors — wrap them in an appropriate `InteractionException`.

### 5. MVC Controllers Stay Thin

Controllers dispatch and map — no business logic. Use `interaction.DispatchOnMapAsync()` for POST handlers to auto-map view model → request and populate `ModelState` from validation errors.

---

## Project Dependency Rules

```
Web.*           → InternalApi.Client, NodeApi.Client (temp), WebFramework, WebFramework.Mvc
InternalApi     → Core.UseCases, Core.Entities, Gateways.*
Core.UseCases   → Core.Contracts, Core.Entities, Core.Interfaces, Base.Framework
Core.Contracts  → Base.Framework
NodeApi.Client  → Base.Framework, Core.Contracts   [TEMPORARY — being deleted]
```

**Never introduce circular dependencies.** `Core.*` projects must not reference `InternalApi.*`, `Web.*`, or `Gateways.*`.

---

## NodeApi.Client — Temporary Code

Everything in `src/Dfe.SignIn.NodeApi.Client/` is **temporary**. These are HTTP wrappers calling the legacy Node.js APIs. They are being replaced one-by-one with proper EF-backed use cases in `Core.UseCases`.

When converting a Node requester:
1. Create the equivalent use case in `Core.UseCases`
2. Register it in `InternalApi` configuration
3. Delete the requester file from `NodeApi.Client`
4. Remove the Node requester registration from consuming apps (`Web.Profile/Program.cs`, etc.)

Do **not** add new code to `NodeApi.Client`.

---

## Where Things Live

| What you're looking for | Where to find it |
|---|---|
| Business contracts (request/response types) | `src/Dfe.SignIn.Core.Contracts/` |
| Business logic | `src/Dfe.SignIn.Core.UseCases/` |
| Database models | `src/Dfe.SignIn.Core.Entities/` |
| Gateway interfaces | `src/Dfe.SignIn.Core.Interfaces/` |
| EF implementation | `src/Dfe.SignIn.Gateways.EntityFramework/` |
| Internal API endpoint registration | `src/Dfe.SignIn.InternalApi/Endpoints/InteractionEndpoints.cs` |
| Use case DI registration | `src/Dfe.SignIn.InternalApi/Configuration/` |
| Shared enum/constants for NuGet consumers | `src/Dfe.SignIn.Core.Public/` |
| Interaction dispatcher base classes | `src/Dfe.SignIn.Base.Framework/` |
| MVC view model mapping utilities | `src/Dfe.SignIn.WebFramework.Mvc/` |
| Help content (Markdown) | `src/Dfe.SignIn.Web.Help/ContentFiles/` |
| External developer docs (Markdown) | `docs/external/` |
| NuGet version pinning | `Directory.Packages.props` |

---

## Testing Conventions

- Framework: `xUnit`
- Test project naming: `Dfe.SignIn.Foo.UnitTests` mirrors `Dfe.SignIn.Foo`
- Mock the interaction dispatcher using `MockInteractionDispatcher` from `TestHelpers`
- Use `interaction.MockResponse(request, response)` — matched by shallow record equality
- Use `interaction.MockThrows<TRequest>(() => exception)` to test exception paths
- Use `interaction.CaptureRequest<TRequest>(list.Add)` to assert what was dispatched (e.g. audit events)

---

## Key Conventions

- **Naming:** Pascal case throughout. Projects use `Dfe.SignIn.*` namespace prefix.
- **Azure Functions prefix:** `Fn.` (e.g. `Fn.AuthExtensions`)
- **Web MVC apps prefix:** `Web.` (e.g. `Web.Help`, `Web.Profile`)
- **Centralised packages:** Version numbers only in `Directory.Packages.props` — never set versions in `.csproj`
- **Local config:** `appsettings.Local.json` + `.NET User Secrets` — never commit secrets
- **Dependencies:** Minimise them. Prefer writing a small helper to importing a new package.
- **Async:** All I/O is async. Accept and pass `CancellationToken` throughout.
- **Nullable:** Enabled project-wide. Use `?` types appropriately.

---

## What NOT to Suggest

- Do not suggest AutoMapper — the project uses its own light mapping convention via `[MapTo]` attributes
- Do not suggest MediatR — the project has its own interaction dispatcher (`Base.Framework`)
- Do not suggest Dapper — EF is the chosen ORM
- Do not add dependencies without strong justification — the philosophy is minimal dependencies
- Do not add comments or docstrings to code you haven't modified
- Do not add error handling for scenarios that cannot occur within the framework's guarantees
