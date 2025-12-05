<#
.SYNOPSIS
    Scaffolds the EF Core DbContext and entity classes for the "Directories" tables.

.DESCRIPTION
    Calls the generic New-DbContext.ps1 script to generate EF Core models and DbContext
    for a given database connection string. This script specifies the project paths,
    namespaces, context name, and tables to scaffold.

.PARAMETER ConnectionString
    The connection string for the database to scaffold.

.EXAMPLE
    ./scripts/migrations/New-DbContextDirectories.ps1 `
        -ConnectionString "Server=.;Database=.;User ID=.;Password=.;TrustServerCertificate=True;"
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]$ConnectionString
)

$ErrorActionPreference = "Stop"

& "$PSScriptRoot/New-DbContext.ps1" `
    -ConnectionString $ConnectionString `
    -ProjectPath "./src/Dfe.SignIn.Gateways.EntityFramework" `
    -EntityPath "./src/Dfe.SignIn.Core.Entities/Directories" `
    -ModelOutputDir "../Dfe.SignIn.Core.Entities/Directories" `
    -ModelNamespace "Dfe.SignIn.Core.Entities.Directories" `
    -ContextOutputDir "./" `
    -ContextNamespace "Dfe.SignIn.Gateways.EntityFramework" `
    -ContextName "DbDirectoriesContext" `
    -Tables @(
    "invitation",
    "invitation_callback",
    "password_history",
    "user",
    "user_code",
    "user_password_history",
    "user_password_policy",
    "user_status_change_reasons"
)
