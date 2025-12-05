<#
.SYNOPSIS
    Generic EF Core database scaffolding script using dotnet-ef.

.DESCRIPTION
    This script scaffolds an Entity Framework Core DbContext and entity
    classes from any SQL Server database. All paths, namespaces,
    context name, tables, and template locations are parameterised so that
    other scripts can call this one for different databases.

.PARAMETER ConnectionString
    The database connection string used for scaffolding.

.PARAMETER ProjectPath
    Path to the .csproj that will receive the generated DbContext.

.PARAMETER ModelOutputDir
    The folder where entity classes should be generated.

.PARAMETER ContextOutputDir
    The folder where the DbContext class should be generated.

.PARAMETER ModelNamespace
    The namespace to apply to the generated model/entity classes.

.PARAMETER ContextNamespace
    The namespace to apply to the generated DbContext class.

.PARAMETER ContextName
    The name of the DbContext class to generate.

.PARAMETER EntityPath
    The path to the Entity classes folder.

.PARAMETER Tables
    The list of database tables to scaffold.

.EXAMPLE
    ./scripts/migrations/New-DbContext.ps1 `
        -ConnectionString "Server=.;Database=.;User ID=.;Password=.;TrustServerCertificate=True;" `
        -ProjectPath "./src/Dfe.SignIn.Gateways.EntityFramework" `
        -EntityPath "./src/Dfe.SignIn.Core.Entities/Directories" `
        -ModelOutputDir "../Dfe.SignIn.Core.Entities/Directories" `
        -ContextOutputDir "./" `
        -ModelNamespace "Dfe.SignIn.Core.Entities.Directories" `
        -ContextNamespace "Dfe.SignIn.Gateways.EntityFramework" `
        -ContextName "DbDirectoriesContext" `
        -Tables @("A","B","C")

#>
[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]$ConnectionString,

    [Parameter(Mandatory)]
    [string]$ProjectPath,

    [Parameter(Mandatory)]
    [string]$ModelOutputDir,

    [Parameter(Mandatory)]
    [string]$ContextOutputDir,

    [Parameter(Mandatory)]
    [string]$ModelNamespace,

    [Parameter(Mandatory)]
    [string]$ContextNamespace,

    [Parameter(Mandatory)]
    [string]$ContextName,

    [Parameter(Mandatory)]
    [string]$EntityPath,

    [string[]]$Tables
)

$ErrorActionPreference = "Stop"

Write-Host "Running EF scaffold for $ContextName..." -ForegroundColor Cyan

# ------------------------------------------------------------
# 1. EF Scaffold Command
# ------------------------------------------------------------
$scaffoldCmd = @(
    "ef", "dbcontext", "scaffold",
    $ConnectionString,
    "Microsoft.EntityFrameworkCore.SqlServer",
    "--project", $ProjectPath,
    "--output-dir", $ModelOutputDir,
    "--context-dir", $ContextOutputDir,
    "--namespace", $ModelNamespace,
    "--context-namespace", $ContextNamespace,
    "--context", $ContextName
    "--no-onconfiguring"
)

# Append tables
foreach ($t in $Tables) {
    $scaffoldCmd += @("--table", $t)
}

dotnet @scaffoldCmd
Write-Host "Scaffold completed successfully." -ForegroundColor Green

# ------------------------------------------------------------
# 2. Rename Model Files to *Entity.cs*
# ------------------------------------------------------------
Write-Host "Renaming generated model files to include 'Entity' suffix..." -ForegroundColor Cyan

Write-Host "Searching in: $EntityPath"

Get-ChildItem -Path $EntityPath -Recurse -Filter '*.cs' |
Where-Object { $_.Name -notmatch 'Entity\.cs$' } |
ForEach-Object {
    $newName = $_.Name -replace '\.cs$', 'Entity.cs'
    Write-Host "Renaming $($_.Name) -> $newName"
    Rename-Item -Path $_.FullName -NewName $newName
}

Write-Host "Renaming complete." -ForegroundColor Green

# ------------------------------------------------------------
# 3. Formatting
# ------------------------------------------------------------
Write-Host "Running dotnet format..." -ForegroundColor Cyan
dotnet format
Write-Host "dotnet format completed." -ForegroundColor Green

Write-Host "Script complete." -ForegroundColor Green
