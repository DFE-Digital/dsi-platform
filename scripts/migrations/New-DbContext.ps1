<#
.SYNOPSIS
    Generic EF Core database scaffolding script using .NET Entity Framework.

.DESCRIPTION
    This script scaffolds an Entity Framework Core DbContext and entity
    classes from any SQL Server database.

.PARAMETER ConnectionString
    The database connection string used for scaffolding.

.PARAMETER Context
    The name of the DbContext configuration to run. Allowed:
    - directories
    - organisations

.EXAMPLE
    ./scripts/migrations/New-DbContext.ps1 `
        -ConnectionString "Server=.;Database=.;User ID=.;Password=.;TrustServerCertificate=True;" `
        -Context directories
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string]$ConnectionString,

    [Parameter(Mandatory)]
    [ValidateSet("directories", "organisations", "audit")]
    [string]$Context
)

$ErrorActionPreference = "Stop"

$DbName = $Context.Substring(0, 1).ToUpper() + $Context.Substring(1)

$profiles = @{
    "directories"   = @{
        Tables = @("invitation", "invitation_callback", "password_history",
            "user", "user_code", "user_password_history",
            "user_password_policy", "user_status_change_reasons")
    }

    "organisations" = @{
        Tables = @()
    }

    "audit"         = @{
        Tables = @("AuditLogs", "AuditLogMeta")
    }
}

$config = $profiles[$Context]

$ModelOutputDir = "../Dfe.SignIn.Core.Entities/$DbName"
$ModelNamespace = "Dfe.SignIn.Core.Entities.$DbName"
$ContextName = "Db$DbName`Context"
$EntityPath = "./src/Dfe.SignIn.Core.Entities/$DbName"

Write-Host "Scaffolding DbContext: $($ContextName)" -ForegroundColor Cyan

# ------------------------------------------------------------
# 1. EF Scaffold Command
# ------------------------------------------------------------
$scaffoldCmd = @(
    "ef", "dbcontext", "scaffold",
    $ConnectionString,
    "Microsoft.EntityFrameworkCore.SqlServer",
    "--project", "./src/Dfe.SignIn.Gateways.EntityFramework",
    "--output-dir", $ModelOutputDir,
    "--context-dir", "./",
    "--namespace", $ModelNamespace,
    "--context-namespace", "Dfe.SignIn.Gateways.EntityFramework",
    "--context", $ContextName,
    "--no-onconfiguring",
    "--force"
)

foreach ($t in $config.Tables) {
    $scaffoldCmd += @("--table", $t)
}

Write-Host "Running EF scaffold..." -ForegroundColor Cyan
dotnet @scaffoldCmd
Write-Host "Scaffold completed." -ForegroundColor Green

# ------------------------------------------------------------
# 2. Rename Files to include "Entity"
# ------------------------------------------------------------
Write-Host "Renaming entity files in '$($EntityPath)'" -ForegroundColor Cyan

Get-ChildItem -Path $EntityPath -Recurse -Filter '*.cs' |
Where-Object { $_.Name -notmatch 'Entity\.cs$' } |
ForEach-Object {
    $newName = $_.Name -replace '\.cs$', 'Entity.cs'
    $targetPath = Join-Path $_.DirectoryName $newName

    if (Test-Path $targetPath) {
        Write-Host "Deleting existing file: $newName"
        Remove-Item -Path $targetPath -Force
    }

    Write-Host "Renaming $($_.Name) -> $newName"
    Rename-Item -Path $_.FullName -NewName $newName
}

Write-Host "Renaming complete." -ForegroundColor Green

# ------------------------------------------------------------
# 3. dotnet format
# ------------------------------------------------------------
Write-Host "Running dotnet format..." -ForegroundColor Cyan
dotnet format
Write-Host "Format completed." -ForegroundColor Green

Write-Host "All done." -ForegroundColor Green
