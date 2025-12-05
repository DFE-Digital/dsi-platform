<#
.SYNOPSIS
    Scaffolds the EF Core DbContext and entity classes for the "Organisations" tables.

.DESCRIPTION
    Calls the generic New-DbContext.ps1 script to generate EF Core models and DbContext
    for a given database connection string. This script specifies the project paths,
    namespaces, context name, and tables to scaffold.

.PARAMETER ConnectionString
    The connection string for the database to scaffold.

.EXAMPLE
    ./scripts/migrations/New-DbContextOrganisations.ps1 `
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
    -EntityPath "./src/Dfe.SignIn.Core.Entities/Organisations" `
    -ModelOutputDir "../Dfe.SignIn.Core.Entities/Organisations" `
    -ModelNamespace "Dfe.SignIn.Core.Entities.Organisations" `
    -ContextOutputDir "./" `
    -ContextNamespace "Dfe.SignIn.Gateways.EntityFramework" `
    -ContextName "DbOrganisationsContext"
