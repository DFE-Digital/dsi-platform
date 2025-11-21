# dsi-platform

[![CI](https://github.com/DFE-Digital/dsi-platform/actions/workflows/ci.yml/badge.svg?event=merge_group)](https://github.com/DFE-Digital/dsi-platform/actions/workflows/ci.yml)

This repository contains the .NET version of the DfE Sign-in platform.Sign-in solution.

## Working on PowerShell scripts locally

The official Microsoft PowerShell extension can be used to ensure automatic formatting and code analysis when using Visual Studio Code.

### Running PowerShell tests locally

1. Install the [Pester](https://pester.dev/) tooling.

2. Run command:

   ```pwsh
   Invoke-Pester scripts
   ```

3. Or, to run with coverage reporting:

   ```pwsh
   $config = New-PesterConfiguration -Hashtable $(Import-PowerShellDataFile ./.config/PesterSettings.psd1)
   Invoke-Pester -Configuration $config
   ```

### PowerShell code quality checks

Code quality checks are provided in real-time by installing the official Microsoft PowerShell extension for Visual Studio Code.

They can also be ran from the command line:

1. Install the [PowerShell/PSScriptAnalyzer](https://github.com/PowerShell/PSScriptAnalyzer) tooling.

2. Run command:

   ```pwsh
   Invoke-ScriptAnalyzer -Path ./scripts/**/*.ps1 -Settings ./.config/PSScriptAnalyzerSettings.psd1
   ```

## Updating the frontend assets version

The latest version of the frontend assets is maintained by developers by updating the `version` property in the `/frontend/package.json` file.

Applications can specify which version of the frontend assets they depend on using the `Assets:FrontendVersion` configuration which is maintained by developers in the various `appsettings.json` files throughout this repository:

- Developer documentation - `/docs/appsettings.json`
- Frontend applications - `/src/Dfe.SignIn.Web.*/appsettings.json`

This approach allows developers to iteratively update, test and rollout frontend udpates across the various frontend applications. This is particuarly useful when a large scale change is being made; for example, a major update to the GOV.UK design system which requires template changes.

> **Note** - If necessary the dev-ops team can override the `Assets:FrontendVersion` of any application by providing this configuration to the deployed app service.

## Pushing NuGet packages to your local feed

It is sometimes useful to push packages to a local feed when working locally.

Setup the local NuGet feed in your local development environment:

```pwsh
mkdir ~/.nuget-local
dotnet nuget add source ~/.nuget-local --name LocalStore
```

Use the following command to build and push NuGet packages to a local feed:

```pwsh
dotnet build /p:BuildPackages=true
dotnet nuget push "packages/*.nupkg" --source LocalStore
```

Alternatively the following can be ran from the Visual Studio Code command palette:

```
task dfe: update local feed
```
