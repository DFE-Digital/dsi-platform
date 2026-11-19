# Dfe.SignIn.AppHost

Aspire 17 AppHost for the configured local .NET and Node.js apps, Redis, and frontend assets.

## Prerequisites

| Requirement                                                                                            | Notes                                                                         |
| ------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------- |
| [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10)                                         | Required to build the AppHost and .NET services                               |
| [Aspire CLI 17](https://aspire.dev/get-started/install-cli/)                                           | Supplies the dashboard and orchestration tools; update separately from NuGet packages |
| [Node.js](https://nodejs.org/)                                                                         | Required for Node platform apps (OIDC, Interactions, Services)                |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/)                                      | Must be running before launching                                              |
| [PowerShell 7+](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell)  | Required for the TLS proxy tool                                               |
| [dsi-tools](https://dfe-secureaccess.atlassian.net/wiki/spaces/NSA/pages/4647682065/Install+dsi-tools) | Required for local TLS proxying (e.g. Profile app). Follow the install guide. |
| User secrets                                                                                           | See section below                                                             |

No Aspire workload is required. Check the installed tools from PowerShell:

```powershell
dotnet --version
aspire --version
```

### Updating Aspire

The project's Aspire package versions are pinned in `Directory.Packages.props`.
The installed CLI has its own version and supplies the dashboard, so updating the
NuGet packages alone does not update the dashboard UI.

To update the CLI, stop this AppHost first, then restart it after updating:

```powershell
# Run from the solution root
aspire stop
aspire update --self
aspire --version
aspire start
```

Run `aspire update` from the solution root when intentionally updating the project's
Aspire packages. Review its changes before committing; it is not needed just to
update the installed CLI.

## Dev setup

Before continuing, complete the standard onboarding steps:

- [Development prerequisites](https://dfe-secureaccess.atlassian.net/wiki/spaces/NSA/pages/4643454992/Development+prerequisites)
- [Developer onboarding](https://dfe-secureaccess.atlassian.net/wiki/spaces/NSA/pages/4642635786/Developer+onboarding)

## Secrets setup

The AppHost reads configuration from its appsettings files and .NET User Secrets,
then passes selected settings to the apps at run time. Do not put secrets in
`appsettings.json` or `appsettings.Local.json`. The launch profiles select the
`Local` environment, whose defaults include placeholder values that must be overridden
for the apps you enable.

If `ConnectionStrings:AppConfiguration` is configured, the AppHost also loads Azure
App Configuration and Key Vault references. `AppConfiguration:Tag` is then required,
along with suitable Azure credentials and access. User Secrets are applied again
after the remote configuration so local overrides take precedence.

Set secrets against the AppHost project (ID: `03d41373-1462-4128-acae-dff999202b39`):

```powershell
dotnet user-secrets set "Oidc:ClientId"           "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "Oidc:ClientSecret"       "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "Oidc:Authority"          "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "Oidc:MetadataAddress"    "<value>" --project src\Dfe.SignIn.AppHost

dotnet user-secrets set "ExternalId:ClientId"     "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "ExternalId:ClientSecret" "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "ExternalId:Authority"    "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "ExternalId:Instance"     "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "ExternalId:TenantId"     "<value>" --project src\Dfe.SignIn.AppHost

dotnet user-secrets set "InternalApiClient:ClientId"                  "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "InternalApiClient:ClientSecret"              "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "InternalApiClient:HostUrl"                   "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "InternalApiClient:Tenant"                    "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "InternalApiClient:Access:BaseAddress"        "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "InternalApiClient:Organisations:BaseAddress" "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "InternalApiClient:Directories:BaseAddress"   "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "InternalApiClient:Applications:BaseAddress"  "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "InternalApiClient:Resource"                  "<value>" --project src\Dfe.SignIn.AppHost

dotnet user-secrets set "Session:DurationInMinutes"                   "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "Session:NotifyRemainingMinutes"              "<value>" --project src\Dfe.SignIn.AppHost

dotnet user-secrets set "BearerToken:ValidAudience"                             "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "PublicApiSecretEncryption:Key"                         "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "SelectOrganisation:SelectOrganisationBaseAddress"      "<value>" --project src\Dfe.SignIn.AppHost

dotnet user-secrets set "EntityFramework:Directories:Host"     "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "EntityFramework:Directories:Name"     "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "EntityFramework:Directories:Username" "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "EntityFramework:Directories:Password" "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "EntityFramework:Organisations:Host"     "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "EntityFramework:Organisations:Name"     "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "EntityFramework:Organisations:Username" "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "EntityFramework:Organisations:Password" "<value>" --project src\Dfe.SignIn.AppHost

dotnet user-secrets set "NodePlatformDirectory" "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "NodeEnvFileName"       "<value>" --project src\Dfe.SignIn.AppHost

dotnet user-secrets set "GovNotify:ApiKey"                                    "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "RaiseSupportTicketByEmail:SupportEmailAddress"       "<value>" --project src\Dfe.SignIn.AppHost
dotnet user-secrets set "RaiseSupportTicketByEmail:EmailTemplateId"           "<value>" --project src\Dfe.SignIn.AppHost
```

> Actual values are shared via the team's secrets store. Ask a team member if you don't have them.

These commands cover common settings, not every configuration value. Check
`appsettings.Local.json` and the environment mappings in `AppHost.cs` and
`ProjectResourceBuilderExtensions.cs` for the apps you enable.

### Node apps

Node apps are disabled in the checked-in configuration. To enable them, configure
`NodePlatformDirectory` to contain `login.dfe.oidc`, `login.dfe.interactions`, and
`login.dfe.services`, and set the relevant `Components:Node:*Enabled` flags.
Use an absolute directory path to avoid working-directory ambiguity.

`NodeEnvFileName` names a shared environment file inside that directory, not inside
each app. It must exist when any Node app is enabled. Use unquoted `KEY=value` lines
with unique keys; the loader does not perform shell expansion or dotenv quote handling.
The AppHost overrides Redis and frontend asset addresses after loading this file.

Dependencies are installed through `WithNpm()`. OIDC and Interactions run the `dev`
npm script; Services runs `start`. Node apps are considered ready when their logs contain
`Dev server listening`; Interactions waits for OIDC when enabled, and Services waits
for both enabled dependencies.

## Running with the CLI

Ensure Docker Desktop is running, then run the following from the **solution root** (where `aspire.config.json` is located). The Aspire CLI uses that file to locate the AppHost project automatically.

```powershell
# Start in the background and return to the prompt
aspire start

# Or run interactively in the current terminal
aspire run
```

Aspire builds the AppHost and enabled .NET project resources, starts the configured
containers, installs enabled Node apps' npm dependencies, and launches the apps.
It does not build every project in the solution. The dashboard login URL is printed
at startup; `aspire start` also prints the log path and AppHost PID.

The .NET resources use Aspire 17's `AddDotnetProject` API. Watch-mode integration
is not yet supported for this resource model, so `dotnet watch` is not a substitute
for rebuilding the hosted apps. After source changes, use the dashboard's Rebuild
command for the affected .NET resource, or stop and start the AppHost again.

To stop this AppHost when it was started in the background:

```powershell
aspire stop
```

For an interactive `aspire run`, press **Ctrl+C**. Redis data is stored in a Docker
volume and persists across normal stop/start cycles.

## Debugging .NET applications

Start the AppHost with `aspire start` or `aspire run` as described above, then attach
your debugger to an already-running application. Do not start a second copy using
an IDE launch configuration or a separate `dotnet run`; this can cause port conflicts
and bypass the environment variables configured by the AppHost.

Ensure the application's component flag is enabled and its resource is running.
The .NET resources are `help`, `profile`, `public-api`, and
`internal-api`. Attach to the application you want to debug, not
`Dfe.SignIn.AppHost` or the `dotnet run` launcher.

### Visual Studio Code: attach through the Aspire view

1. Open the repository in a trusted VS Code workspace.
2. Install the Microsoft **Aspire** extension version **1.25.0 or later**, and the
   Microsoft **C#** extension (also installed by **C# Dev Kit**).
3. Open the **Aspire** view and expand the AppHost started from the CLI.
4. Right-click the running .NET resource and select **Attach debugger**.
5. Wait for attachment to complete, set breakpoints, then make a request to the
   application's endpoint shown in the dashboard.

Use **Disconnect** on the debug toolbar to detach without stopping the application.
Attaching does not restart the resource, so code that ran during startup will not
be hit by breakpoints set after attachment.

See [Aspire's debugger attachment documentation](https://aspire.dev/get-started/aspire-vscode-extension/#attach-a-debugger-to-a-running-resource).

### Visual Studio 2026: attach to the application process

1. Open `dsi-platform.sln` and the source file you want to debug.
2. Select **Debug > Attach to Process** (**Ctrl+Alt+P**) and use the local connection.
3. Select the application's process, such as `Dfe.SignIn.Web.Profile.exe`, and attach
   using the managed .NET debugger.
4. Set breakpoints and make a request to the application's dashboard endpoint.

If needed, identify the process from PowerShell:

```powershell
Get-CimInstance Win32_Process -Filter "Name LIKE 'Dfe.SignIn.%.exe' OR Name = 'dotnet.exe'" |
    Select-Object ProcessId, Name, CommandLine
```

For applications hosted by `dotnet.exe`, select the process whose command line
contains the application's DLL path, not one containing `dotnet run`.
The PID printed by `aspire start` belongs to the AppHost, not the application.
Use **Debug > Detach All** when finished to leave the applications running.

### Breakpoints and rebuilding

If a breakpoint is unbound, confirm that you attached to the correct process and
that the loaded assembly and PDB symbols match your source. Check Visual Studio's
**Debug > Windows > Modules** window or VS Code's debugger diagnostics for symbol
loading problems.

After rebuilding or restarting a resource, its process can change. Reattach to the
new process before debugging again. Do not rely on watch mode for these
`AddDotnetProject` resources.

## What gets started

`AppHost.cs` defines resource registrations, component switches, tags and dependencies.
Configuration-provider setup lives in `AppHostConfigurationExtensions.cs`. Reusable .NET settings
helpers live in `ProjectResourceBuilderExtensions.cs`, grouped by concern (for example,
`WithEfSettings`, `WithInternalApiSettings`, `WithOidcSettings` and `WithGovNotifySettings`).
Each application explicitly composes the settings it needs in `AppHost.cs`.
`AddDotnetPlatformApp` applies the shared HTTP launch profile, Dotnet tag and default
environment. `RedisResourceBuilderExtensions.cs` contains Redis/Insight registration
and the .NET (`host:port`) and Node (`redis://host:port`) connection-string helpers.
Node setup and frontend asset configuration remain in `NodeResourceBuilderExtensions.cs`
and `FrontendResourceBuilderExtensions.cs`.

The defaults below are from the checked-in `appsettings.json`; local or remote
configuration can override them.

| Resource | Tag | Description | Configuration flag | Checked-in default |
| --- | --- | --- | --- | --- |
| `redis` | Infrastructure | Redis with a persistent data volume and Redis Insight UI | Always added | On |
| `frontend` | Infrastructure | Asset server built from `docker\frontend\Dockerfile` | Always added, including hosted-assets mode | On |
| `help` | Dotnet | Help web app | `Components:DotNet:HelpEnabled` | On |
| `profile` | Dotnet | Profile web app | `Components:DotNet:ProfileEnabled` | On |
| `public-api` | Dotnet | Public API | `Components:DotNet:PublicApiEnabled` | On |
| `internal-api` | Dotnet | Internal API | Always added | On |
| `oidc` | Node | Node OIDC app on HTTPS port 4436 | `Components:Node:OidcEnabled` | Off |
| `interactions` | Node | Node Interactions app on HTTPS port 4431 | `Components:Node:InteractionsEnabled` | Off |
| `services` | Node | Node Services app on HTTPS port 41012 | `Components:Node:ServicesEnabled` | Off |
| `tls-proxy` | Infrastructure | Runs the `Start-DsiTlsProxy` PowerShell function | `Components:Node:Run-TlsProxy` | Off |

Resource names no longer use category prefixes. The Redis volume name is generated
from the application and resource names, so this rename uses a new Redis data volume.
Any volume from the old resource name is left intact but is not automatically reused.

Use User Secrets for personal component choices rather than changing shared defaults:

```powershell
dotnet user-secrets set "Components:Node:OidcEnabled" "true" --project src\Dfe.SignIn.AppHost
```

The Select Organisation app is not registered in this AppHost.

## Frontend assets

Both .NET and Node apps use `WithFrontendAssets(frontend, builder.Configuration)`.
The overloads share address selection and dependency wiring in `FrontendResourceBuilderExtensions`.
Set `Components:Frontend:UseHostedAssets` to `true` (the checked-in default) to use `Assets:BaseAddress`,
or `false` to use the local `frontend` HTTP endpoint.
Node apps require a non-empty `Assets:BaseAddress` when hosted assets are enabled.

For Node apps, this sets `Assets__BaseAddress`, `CDN_BASE_ADDRESS`, and `CDN_HOST_NAME`,
overriding values loaded from the Node environment file. Node apps use the asset URL
directly; `Assets:FrontendVersion` remains a .NET-only setting.

In both modes, apps wait for `frontend`, matching the existing .NET behavior.
Hosted-assets mode does not disable the local container. `Assets:BaseAddress` must
point to an independently reachable asset server; the `Local` defaults use
`http://localhost:8081`, which is not the dynamically allocated Aspire frontend endpoint.

## Aspire dashboard

Open the login URL printed at startup. The checked-in `https` launch profile uses
`https://localhost:17057`; the `http` profile uses `http://localhost:15142`.
Ports can differ when launch settings are overridden or isolated mode is used.
The login token grants dashboard access; do not share the token-bearing URL.

Aspire 17 provides a searchable resource table, an application overview, resource
details, logs, traces, metrics, and a dedicated Terminals page for supported resources.
Structured logs, traces, and metrics require apps to send telemetry; starting a
resource does not automatically instrument it.

Select **Group resources > Tags** beside **Filters** to group resources by
**Infrastructure**, **Dotnet**, and **Node**. The dashboard remembers this selection
across navigation and reloads; adding tags in code does not select the grouping mode.
Tag names are defined once as constants in `ResourceTags.cs` and applied
with `WithTags`, where the experimental `ASPIRERESOURCETRAITS001` diagnostic is suppressed.

Export the running application's resource graph from the dashboard or the solution root:

```powershell
aspire describe --format mermaid
```

For further reference, see the [Aspire 17 release notes](https://aspire.dev/whats-new/aspire-17/)
and [dashboard documentation](https://aspire.dev/dashboard/explore/).

## Troubleshooting

**Docker containers won't start** — ensure Docker Desktop is running before launching the AppHost.

**Old dashboard or upgrade notice after updating packages** — check `aspire --version`.
Update the CLI with `aspire update --self`, then stop and restart the AppHost and open
the newly printed login URL. If the CLI version is still old, use `Get-Command aspire -All`
to check for multiple installations on `PATH`.

**`Start-DsiTlsProxy` not found** — ensure you have the correct PowerShell profile or module loaded. Ask a team member for the required module.

**Secrets not applying** — verify you set secrets against the AppHost project, not an individual app project. Run `dotnet user-secrets list --project src\Dfe.SignIn.AppHost` locally to confirm; its output contains secret values, so do not paste it into logs or issues. Restart the affected resources after changing configuration.

**Node environment file not found** — check `NodePlatformDirectory` and `NodeEnvFileName`,
or disable the Node components you do not need.

**Missing frontend assets** — check `Components:Frontend:UseHostedAssets` and
`Assets:BaseAddress`. Set the flag to `false` to use the Aspire-managed frontend endpoint.

**Port conflicts** — if a port is already in use, stop the conflicting process or adjust `launchSettings.json`.
