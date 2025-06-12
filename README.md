# dsi-platform

[![CI](https://github.com/DFE-Digital/dsi-platform/actions/workflows/ci.yml/badge.svg)](https://github.com/DFE-Digital/dsi-platform/actions/workflows/ci.yml)

This repository contains the .NET version of the DfE Sign-in platform.Sign-in solution.

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
