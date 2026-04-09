# DSI Platform – Project Overview

> Quick-reference for the afternoon call. Not a full README.

---

## What is this?

A .NET rewrite of the **DfE Sign-in (DSI)** platform, currently running on Node.js. The goal is to replace five Node APIs and their frontend components with a clean, unified .NET solution — using Clean Architecture principles.

---

## Current State (Production)

| Component | Status |
|---|---|
| `Fn.AuthExtensions` (Azure Function) | **Live in production** |
| `Web.Help` | Ready — awaiting infrastructure/config to go to pre-prod |
| `Web.Profile` | Ready — awaiting infrastructure/config to go to pre-prod |
| `Web.SelectOrganisation` | Deployed to lower envs, in testing |
| `InternalApi` | Running in lower envs (Tran/Dev) |
| `PublicApi` (.NET) | V2 endpoints live, shadows Node public API via Azure Front Door |

---

## What Still Needs Migrating

### Priority: Node API Requesters → Use Cases

The `NodeApi.Client` project contains ~15 "Node requesters" — temporary wrappers that call the existing Node APIs. **These must be converted to proper .NET use cases** backed by Entity Framework:

- Users: `GetUserProfile`, `ChangeJobTitle`, `ChangeName`, `ChangePassword`, `ChangeEmailAddress` (initiate/confirm/cancel), `CreateUser`, `LinkEntraUser`, `CompleteAnyPendingInvitation`, `GetUserStatus`
- Applications: `GetApplicationByClientId`

Once all are converted → **delete `Dfe.SignIn.NodeApi.Client`** entirely.

### Then: Migrate Node Public API V1 Endpoints to .NET

V1 endpoints stay functional but get rewritten as thin wrappers around the new V2 use cases. Once done → decommission Node public API.

---

## Architecture in Brief

```
[ Web.Help / Web.Profile / Web.SelectOrganisation ]   ← MVC frontends
        ↓ dispatch (IInteractor)
[ InternalApi.Client ]   ← serialises request → HTTP → deserialises response
        ↓
[ InternalApi ]   ← single POST endpoint, dispatches to registered use cases
        ↓
[ Core.UseCases ]   ← business logic, backed by Entity Framework
        ↓
[ Core.Entities / Gateways ]   ← EF models, Redis, ServiceBus, GovNotify
```

**Key principle:** Every business operation is a `Request → Response` type pair. Controllers and use cases communicate exclusively through this dispatch mechanism. No direct service dependencies between layers.

---

## NuGet Packages (for external consumers)

Four packages published to Azure Artifacts (internal) — future target is NuGet.org:

- `Dfe.SignIn.Base.Framework` — interaction dispatcher (targets `net8.0` + `netstandard2.0`)
- `Dfe.SignIn.Core.Public` — shared enum/constant types
- `Dfe.SignIn.PublicApi.Client` — REST client for public API
- `Dfe.SignIn.PublicApi.Client.AspNetCore` — middleware for relying parties

---

## Key Technical Decisions

- **Entity Framework** over Dapper — consistent with wider DfE
- **Clean Architecture** (Uncle Bob) — use cases, interfaces, gateways separated
- **Centralised package management** — `Directory.Packages.props`
- **Single Internal API endpoint** — GraphQL-inspired, all requests go to one POST handler
- **No auto-commit** — GitHub merge queue enforces all unit tests pass before merge
- **Workflow CI** — affected-project detection; only builds/tests what changed on PRs

---

## Immediate Actions for New Team

1. Convert `NodeApi.Client` requesters to use cases (highest priority)
2. Get `Web.Help` and `Web.Profile` deployed to pre-prod (infra tickets open)
3. Add V1 public API endpoint coverage to `Web.Profile` and update `/docs/external` API docs
4. Consider moving to Azure App Configuration for secrets management
5. Publish NuGet packages to NuGet.org (coordinate with Mahmoud Sultan)
