# DSI Platform – Developer Guide

## Repository Structure

```
dsi-platform/
├── src/                    # All source projects
├── tests/                  # All unit test projects
├── solutions/              # blank.sln (CI use), dsi-packages.sln (NuGet packages)
├── docs/                   # DocFX documentation (external + internal)
├── docker/                 # Dockerfiles for docs and frontend assets
├── frontend/               # Frontend assets (JS/CSS per component)
├── scripts/                # PowerShell CI/build scripts
├── migrations/             # DB migration scripts (manual, pre-EF code-first)
├── dsi-platform.sln        # Main solution — brings in everything
├── Directory.Packages.props # Centralised NuGet version management
└── Directory.Build.props
```

---

## Projects at a Glance

### Foundation

| Project | Purpose |
|---|---|
| `Base.Framework` | Interaction dispatcher (Clean Architecture core). Targets `net8.0` + `netstandard2.0`. |
| `Core.Contracts` | Request/Response record types (data annotations for validation). Business domain contracts. |
| `Core.Entities` | Entity Framework model types (DB-first today, code-first planned). |
| `Core.Interfaces` | Gateway abstractions (Redis, ServiceBus, auditing, etc.). |
| `Core.Public` | Shared enums/constants exposed to NuGet consumers. |
| `Core.UseCases` | Business logic interactors. Each use case = one `Request → Response` pair. |

### Infrastructure / Gateways

| Project | Purpose |
|---|---|
| `Gateways.EntityFramework` | EF DbContext implementations. |
| `Gateways.DistributedCache` | Redis cache gateway. |
| `Gateways.ServiceBus` | Azure Service Bus gateway (auditing). |
| `Gateways.GovNotify` | GOV.UK Notify email gateway. |

### APIs

| Project | Purpose |
|---|---|
| `InternalApi` | Single POST endpoint that dispatches to registered use cases. Swagger UI included. |
| `InternalApi.Contracts` | Shared wrapper types between InternalApi and its client. |
| `InternalApi.Client` | HTTP client that serialises/dispatches any request to InternalApi. |
| `NodeApi.Client` | **TEMPORARY** — Node API requesters. Will be deleted once all use cases are migrated. |
| `PublicApi` | Hosted public API. V2 endpoints are .NET. V1 still served from Node (via Front Door routing). |
| `PublicApi.Client` | NuGet package — REST client for external consumers. |
| `PublicApi.Client.AspNetCore` | NuGet package — middleware for .NET relying parties. |
| `PublicApi.Contracts` | Request/response types for the public API. |

### Web Applications

| Project | Purpose |
|---|---|
| `Web.Help` | Help/content MVC app. Content in Markdown, single template renders all pages. |
| `Web.Profile` | User profile management MVC app. |
| `Web.SelectOrganisation` | Organisation selection flow MVC app. |

### Shared Web

| Project | Purpose |
|---|---|
| `WebFramework` | Shared middleware, auth, security headers, session handling. |
| `WebFramework.Mvc` | MVC-specific extensions (view model mapping, model state integration). |

### Azure Functions

| Project | Purpose |
|---|---|
| `Fn.AuthExtensions` | Azure Function with three handlers: `OnTokenIssuanceStart`, `OnAttributeCollectionStart`, `OnAttributeCollectionSubmit`. Handles Entra ID custom auth extensions. **Currently the only .NET component in production.** |

---

## Core Pattern: Interactions

Every business operation follows this pattern:

```csharp
// 1. Define contract in Core.Contracts
public record ChangeJobTitleRequest(Guid UserId, string NewJobTitle) : IInteractionRequest;
public record ChangeJobTitleResponse() : IInteractionResponse;

// 2. Implement use case in Core.UseCases
public sealed class ChangeJobTitleUseCase(IUnitOfWorkDirectories unitOfWork, IInteractionDispatcher interaction)
    : Interactor<ChangeJobTitleRequest, ChangeJobTitleResponse>
{
    public override async Task<ChangeJobTitleResponse> InvokeAsync(
        InteractionContext<ChangeJobTitleRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors(); // validates data annotations on request
        // ... EF logic ...
        return new ChangeJobTitleResponse();
    }
}

// 3. Register in InternalApi configuration
services.AddInteractor<ChangeJobTitleRequest, ChangeJobTitleUseCase>();

// 4. Expose via InternalApi endpoint mapping
app.Map<ChangeJobTitleRequest, ChangeJobTitleResponse>();

// 5. Call from any component via dispatcher
var response = await interaction.DispatchAsync(new ChangeJobTitleRequest(userId, newTitle));
```

**Key rules:**
- Request/Response types must be `record` classes (enables shallow comparison for mocking)
- Always start use case logic with `context.ThrowIfHasValidationErrors()`
- Exceptions must extend `InteractionException` or `NotFoundInteractionException` — these cross the API boundary cleanly
- Annotate exception properties with `[Persist]` to include them in serialised error responses

---

## Adding a New Use Case (Checklist)

1. Add `Request` and `Response` record types in `Core.Contracts` (appropriate namespace)
2. Add data annotation validation to the `Request` type
3. Implement `Interactor<TRequest, TResponse>` in `Core.UseCases`
4. Register with `services.AddInteractor<TRequest, TUseCase>()` in `InternalApi` configuration
5. Map to Swagger UI with `app.Map<TRequest, TResponse>()` in `InteractionEndpoints`
6. Write unit tests in `Core.UseCases.UnitTests`

---

## Converting a Node Requester to a Use Case

The `NodeApi.Client` project contains temporary wrappers that call Node APIs. Pattern for converting:

1. Find the requester in `src/Dfe.SignIn.NodeApi.Client/`
2. Create a new use case in `src/Dfe.SignIn.Core.UseCases/` — same logic, replace HTTP calls with EF queries
3. Register the new use case in `InternalApi` and remove the Node requester registration
4. Delete the requester file from `NodeApi.Client`

Once all requesters are gone → delete `Dfe.SignIn.NodeApi.Client` project entirely.

**Remaining Node requesters to convert:**

- Users: `GetUserProfile`, `ChangeName`, `ChangePassword`, `InitiateChangeEmailAddress`, `ConfirmChangeEmailAddress`, `CancelPendingChangeEmailAddress`, `GetPendingChangeEmailAddress`, `CreateUser`, `LinkEntraUserToDsi`, `CompleteAnyPendingInvitation`, `GetUserStatus`
- Applications: `GetApplicationByClientId`
- Organisations: (check `NodeApi.Client/Organisations/`)
- Access: (check `NodeApi.Client/Access/`)

---

## MVC Controller Pattern

```csharp
// GET — dispatch and hydrate view model
public async Task<IActionResult> Get()
{
    var response = await _interaction.DispatchAsync(new GetSomethingRequest());
    return View(new SomeViewModel { Data = response.Data });
}

// POST — map view model to request, dispatch, handle validation
[HttpPost]
public async Task<IActionResult> Post(SomeViewModel viewModel)
{
    if (!await interaction.DispatchOnMapAsync(viewModel, out var request, ModelState))
        return View(viewModel); // validation failed, re-render with errors

    await interaction.DispatchAsync(request with { UserId = User.GetUserId() });
    return RedirectToAction("Success");
}
```

View model properties use `[MapTo(typeof(SomeRequest), nameof(SomeRequest.Property))]` to wire up the auto-mapping.

---

## Running Locally

### Prerequisites
- .NET 8 SDK
- Docker Desktop (for frontend assets and docs)
- DSI Tools CLI (`dsi` command)
- Azure CLI (for `dsi connect`)

### Steps

1. **Connect to an environment** (dev or tran):
   ```pwsh
   dsi connect
   ```

2. **Import secrets:**
   ```pwsh
   dsi import secrets
   ```

3. **Run a component** — use the launch profile in `appsettings.Local.json`. VS Code launch configs are provided.

4. **Frontend assets** — must be running alongside any web component:
   ```pwsh
   # From VS Code: run the "Frontend Assets" task
   # Or via Docker directly
   ```

### Getting an Internal API Token (for Swagger)

```pwsh
dsi connect  # connect to tran or dev
dsi internal-api token
# paste the output into Swagger UI → Authorise
```

When running **locally**, no token is needed — Swagger uses your local secrets automatically.

---

## Testing

### Unit Tests

Uses `xUnit`. Test projects mirror source projects: `Dfe.SignIn.Foo.UnitTests`.

**Mocking interactions:**
```csharp
// Arrange
var interaction = new MockInteractionDispatcher();
interaction.MockResponse(
    new GetUserStatusRequest(userId),
    new GetUserStatusResponse(IsActive: true)
);

// Act
var useCase = new SomeUseCase(interaction);
await useCase.InvokeAsync(context);

// Assert
interaction.MockThrows<GetUserStatusRequest>(() => new UserNotFoundException());
```

**Capturing requests for audit assertions:**
```csharp
var captured = new List<WriteToAuditRequest>();
interaction.CaptureRequest<WriteToAuditRequest>(captured.Add);
// ... act ...
Assert.Contains(captured, r => r.EventCategory == AuditEventCategoryNames.ChangeJobTitle);
```

### Running Tests

```pwsh
dotnet test dsi-platform.sln
```

CI only runs tests for affected projects on PRs (PowerShell scripts determine dependency graph). Running manually or on merge queue runs all tests.

---

## CI / GitHub Workflows

- **On PR:** detects affected projects, builds and tests only those + their dependents
- **On merge queue:** runs all unit tests
- **Manual dispatch:** choose environment (dev/tran), select whether to deploy
- **Blank solution** (`solutions/blank.sln`) is used by workflows to dynamically add only affected projects

Workflows live in `.github/workflows/`. Key file: `ci.yml`.

---

## Configuration & Secrets

- Local: `.NET User Secrets` (`appsettings.Local.json` + `dsi import secrets`)
- Deployed: Environment variables from Azure DevOps bicep parameters (`double__` separator for hierarchy)
- Static non-secret config: `appsettings.json` (can be overridden by env vars)
- Future recommendation: **Azure App Configuration** (hierarchical config, Key Vault integration, no restart needed)

---

## Documentation

Developer docs are generated with **DocFX** using a custom GDS-styled theme.

```pwsh
# View external docs (for relying parties)
# Run "External Docs" VS Code task — starts Docker with DocFX site

# View internal docs (full API reference)
# Run "Internal Docs" VS Code task
```

Markdown files live in `docs/external/` and `docs/internal/`. Namespace documentation lives in `docs/namespaces/`.

---

## NuGet Packages

Four projects are packaged for external use:

| Package | Target |
|---|---|
| `Dfe.SignIn.Base.Framework` | `net8.0` + `netstandard2.0` |
| `Dfe.SignIn.Core.Public` | `net8.0` + `netstandard2.0` |
| `Dfe.SignIn.PublicApi.Client` | `net8.0` + `netstandard2.0` |
| `Dfe.SignIn.PublicApi.Client.AspNetCore` | `net8.0` |

**Current registry:** Azure Artifacts (internal, for automated testing)
**Future registry:** NuGet.org (coordinate with Mahmoud Sultan for DfE org account)

Build and push locally:
```pwsh
dotnet build /p:BuildPackages=true
dotnet nuget push "packages/*.nupkg" --source LocalStore
```

---

## Help Component — Content Authoring

Content lives in `src/Dfe.SignIn.Web.Help/ContentFiles/` as Markdown with YAML front matter:

```yaml
---
title: Changing your email address
caption: Your account
updatedDate: 2024-11-01
---

Content here in standard Markdown...
```

The `Reload` button in the local dev toolbar refreshes content without restarting the app.

The index page cards are configured in a YAML file — cards are ordered by declaration order. Title and summary are pulled from content front matter.
