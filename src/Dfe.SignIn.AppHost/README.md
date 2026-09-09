# Dfe.SignIn.AppHost

.NET Aspire host that orchestrates the full local development environment — Redis, frontend assets, and all web apps.

## Prerequisites

| Requirement                                                                                            | Notes                                                                         |
| ------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------- |
| [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10)                                         | Including the Aspire workload (`dotnet workload install aspire`)              |
| [Aspire CLI](https://aspire.dev/get-started/install-cli/)                                              | Required to run and manage the Aspire app host                                |
| [Node.js](https://nodejs.org/)                                                                         | Required for Node platform apps (OIDC, Interactions, Services)                |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/)                                      | Must be running before launching                                              |
| [PowerShell 7+](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell)  | Required for the TLS proxy tool                                               |
| [dsi-tools](https://dfe-secureaccess.atlassian.net/wiki/spaces/NSA/pages/4647682065/Install+dsi-tools) | Required for local TLS proxying (e.g. Profile app). Follow the install guide. |
| User secrets                                                                                           | See section below                                                             |

## Dev setup

Before continuing, complete the standard onboarding steps:

- [Development prerequisites](https://dfe-secureaccess.atlassian.net/wiki/spaces/NSA/pages/4643454992/Development+prerequisites)
- [Developer onboarding](https://dfe-secureaccess.atlassian.net/wiki/spaces/NSA/pages/4642635786/Developer+onboarding)

## Secrets setup

The AppHost reads secrets from .NET User Secrets and distributes them to each app at run time. No secrets should go into `appsettings.Local.json`.

Set secrets against the AppHost project (ID: `03d41373-1462-4128-acae-dff999202b39`):

```bash
dotnet user-secrets set "Oidc:ClientId"           "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "Oidc:ClientSecret"       "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "Oidc:Authority"          "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "Oidc:MetadataAddress"    "<value>" --project src/Dfe.SignIn.AppHost

dotnet user-secrets set "ExternalId:ClientId"     "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "ExternalId:ClientSecret" "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "ExternalId:Authority"    "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "ExternalId:Instance"     "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "ExternalId:TenantId"     "<value>" --project src/Dfe.SignIn.AppHost

dotnet user-secrets set "InternalApiClient:ClientId"                  "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "InternalApiClient:ClientSecret"              "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "InternalApiClient:HostUrl"                   "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "InternalApiClient:Tenant"                    "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "InternalApiClient:Access:BaseAddress"        "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "InternalApiClient:Organisations:BaseAddress" "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "InternalApiClient:Directories:BaseAddress"   "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "InternalApiClient:Applications:BaseAddress"  "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "InternalApiClient:Search:BaseAddress"        "<value>" --project src/Dfe.SignIn.AppHost

dotnet user-secrets set "Session:DurationInMinutes"                   "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "Session:NotifyRemainingMinutes"              "<value>" --project src/Dfe.SignIn.AppHost

dotnet user-secrets set "BearerToken:ValidAudience"                             "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "PublicApiSecretEncryption:Key"                         "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "SelectOrganisation:SelectOrganisationBaseAddress"      "<value>" --project src/Dfe.SignIn.AppHost

dotnet user-secrets set "EntityFramework:Directories:Host"     "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "EntityFramework:Directories:Name"     "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "EntityFramework:Directories:Username" "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "EntityFramework:Directories:Password" "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "EntityFramework:Organisations:Host"     "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "EntityFramework:Organisations:Name"     "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "EntityFramework:Organisations:Username" "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "EntityFramework:Organisations:Password" "<value>" --project src/Dfe.SignIn.AppHost

dotnet user-secrets set "NodePlatformDirectory" "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "NodeEnvFileName"       "<value>" --project src/Dfe.SignIn.AppHost

dotnet user-secrets set "GovNotify:ApiKey"                                    "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "RaiseSupportTicketByEmail:SupportEmailAddress"       "<value>" --project src/Dfe.SignIn.AppHost
dotnet user-secrets set "RaiseSupportTicketByEmail:EmailTemplateId"           "<value>" --project src/Dfe.SignIn.AppHost
```

> Actual values are shared via the team's secrets store. Ask a team member if you don't have them.

## Running

### Visual Studio 2026

1. Open `dsi-platform.sln`.
2. Ensure Docker Desktop is running.
3. Set `Dfe.SignIn.AppHost` as the startup project.
4. Press **F5** (or **Ctrl+F5** to run without debugging).

The Aspire dashboard will open automatically in your browser.

### Visual Studio Code

1. Open the repo root in VS Code.
2. Install the recommended extensions when prompted (or from `.vscode/extensions.json`).
3. Open the **Run and Debug** panel and start one of the individual app configurations, or use the **Aspire** run configuration if available via the C# Dev Kit extension.

Alternatively, use the terminal option below.

### Command line

Ensure Docker Desktop is running, then run the following from the **solution root** (where `aspire.config.json` is located). The Aspire CLI uses that file to locate the AppHost project automatically.

```bash
# Using the Aspire CLI (recommended)
aspire start

# Using the .NET CLI
dotnet watch run --project src/Dfe.SignIn.AppHost
```

This will build the solution, start Docker containers (Redis, frontend), and launch all configured apps. The Aspire dashboard URL is printed to the console on startup.

To stop all resources when using the Aspire CLI:

```bash
aspire stop
```

## What gets started

| Resource            | Description                                                                                               |
| ------------------- | --------------------------------------------------------------------------------------------------------- |
| `infra-redis`       | Redis instance with a persistent data volume + Redis Commander UI                                         |
| `infra-frontend`    | Frontend asset server built from `docker/frontend/Dockerfile`                                             |
| `app-help`          | Help web app — conditional on `Components:DotNet:HelpEnabled` (default: true)                             |
| `app-profile`       | Profile web app — conditional on `Components:DotNet:ProfileEnabled` (default: true)                       |
| `app-public-api`    | Public API — conditional on `Components:DotNet:PublicApiEnabled` (default: true)                          |
| `app-internal-api`  | Internal API — always started                                                                             |
| `node-oidc`         | Node OIDC app on port 4436 — conditional on `Components:Node:OidcEnabled` (default: true)                 |
| `node-interactions` | Node Interactions app on port 4431 — conditional on `Components:Node:InteractionsEnabled` (default: true) |
| `node-services`     | Node Services app on port 41012 — conditional on `Components:Node:ServicesEnabled` (default: true)        |
| `tool-tls-proxy`    | Local TLS proxy (`Start-DsiTlsProxy` PowerShell function)                                                 |

## Aspire dashboard

The dashboard is available at the URL printed to the console on startup (typically `http://localhost:15142`). Use it to view logs, traces, and the state of all resources.

For further reference, see the [Aspire documentation](https://aspire.dev/docs/).

## Troubleshooting

**Docker containers won't start** — ensure Docker Desktop is running before launching the AppHost.

**`Start-DsiTlsProxy` not found** — ensure you have the correct PowerShell profile or module loaded. Ask a team member for the required module.

**Secrets not applying** — verify you set secrets against the AppHost project, not an individual app project. Run `dotnet user-secrets list --project src/Dfe.SignIn.AppHost` to confirm.

**Port conflicts** — if a port is already in use, stop the conflicting process or adjust `launchSettings.json`.
