<#
.SYNOPSIS
    Scaffold an initial Entity Framework Core migration for a given DbContext.

.DESCRIPTION
    This script uses the Entity Framework Core CLI (`dotnet ef`) to create an initial migration
    for a specified DbContext.

.PARAMETER Context
    The name of the DbContext class for which to scaffold the migration.
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [ValidateSet("directories", "organisations")]
    [string]$Context
)

$ErrorActionPreference = "Stop"

$DbName = $Context.Substring(0, 1).ToUpper() + $Context.Substring(1)
$ContextName = "Db$DbName`Context"

Write-Host "Running initial create for $ContextName..." -ForegroundColor Cyan

$scaffoldCmd = @(
    "ef", "migrations", "add", "InitialCreate",
    "--project", "./migrations/migrations.csproj",
    "--output-dir", $DbName,
    "--context", $ContextName
)

dotnet @scaffoldCmd

Write-Host "Completed successfully." -ForegroundColor Green
