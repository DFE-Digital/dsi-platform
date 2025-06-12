# dsi-platform

[![CI](https://github.com/DFE-Digital/dsi-platform/actions/workflows/ci.yml/badge.svg)](https://github.com/DFE-Digital/dsi-platform/actions/workflows/ci.yml)

This repository contains the .NET version of the DfE Sign-in platform.Sign-in solution.

# Running PowerShell tests locally

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


# PowerShell code quality checks

Code quality checks are provided in real-time by installing the official Microsoft PowerShell extension for Visual Studio Code.

They can also be ran from the command line:

1. Install the [PowerShell/PSScriptAnalyzer](https://github.com/PowerShell/PSScriptAnalyzer) tooling.

2. Run command:

    ```pwsh
    Invoke-ScriptAnalyzer -Path ./scripts/**/*.ps1 -Settings ./.config/PSScriptAnalyzerSettings.psd1
    ```

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
dotnet nuget push "**/*.nupkg" --source LocalStore
```

Alternatively the following can be ran from the Visual Studio Code command palette:

```
task dfe: update local feed
```
