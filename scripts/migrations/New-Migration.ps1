<#
.SYNOPSIS
    Creates a new Entity Framework Core migration for a specified DbContext.

.DESCRIPTION
    This script uses the Entity Framework Core CLI (`dotnet ef`) to scaffold a new migration
    for a given DbContext. The user can specify the context, the target folder for the migration,
    and the migration name.

.PARAMETER Context
    The name of the DbContext class for which to create the migration.

.PARAMETER MigrationName
    The name of the migration to create e.g. "AddEmailToWorker".
    This will also be the class name of the generated migration.
#>

[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [ValidateSet("directories", "organisations", "audit")]
    [string]$Context,

    [Parameter(Mandatory)]
    [string]$MigrationName
)

$ErrorActionPreference = "Stop"

$DbName = $Context.Substring(0, 1).ToUpper() + $Context.Substring(1)
$ContextName = "Db$DbName`Context"

Write-Host "Adding new migration $MigrationName..." -ForegroundColor Cyan

$scaffoldCmd = @(
    "ef", "migrations", "add",
    $MigrationName,
    "--project", "./migrations/migrations.csproj",
    "--output-dir", $DbName,
    "--context", $ContextName
)

dotnet @scaffoldCmd

Write-Host "Completed successfully." -ForegroundColor Green
