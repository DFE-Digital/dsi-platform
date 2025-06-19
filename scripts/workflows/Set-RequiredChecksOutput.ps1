<#
.SYNOPSIS
    Set workflow step output flags to specify which checks need to be performed.

.PARAMETER ChangedFiles
    The list of zero-or-more changed files.

.PARAMETER ForceCheckPowershell
    A value indicating whether PowerShell checks should be performed regardless of whether
    any PowerShell scripts have been changed.

.PARAMETER ForceCheckDotnet
    A value indicating whether .NET checks should be performed regardless of whether
    any .NET source or test files have been changed.

.PARAMETER ForceCheckDocs
    A value indicating whether developer reference documentation checks should be performed
    regardless of whether any associated files have been changed.

.PARAMETER ForceBuildExternalDocs
    A value indicating whether the external developer reference documentation needs to be
    build regardless of whether any associated documentation or template files have been
    changed.

.PARAMETER ForcePublishDotnetPackages
    A value indicating whether NuGet packages need to be built and published regardless of
    whether any related package source files have been changed.

.OUTPUTS
    GitHub outputs:
        - check_powershell - 'true' for PowerShell checks.
        - check_dotnet - 'true' for .NET checks.
        - check_docs_templates - 'true' for documentation checks.
        - build_external_docs - 'true' to build external developer reference documentation.
        - publish_dotnet_packages - 'true' to publish NuGet packages.

.EXAMPLE
    $changedFiles = ./scripts/workflows/Get-ChangedFilesInBranch
    ./scripts/workflows/Set-RequiredChecksOutput `
        -ChangedFiles $changedFiles

.EXAMPLE
    $changedFiles = ./scripts/workflows/Get-ChangedFilesInBranch
    ./scripts/workflows/Set-RequiredChecksOutput `
        -ChangedFiles $changedFiles `
        -ForceCheckPowershell true
#>
[CmdletBinding()]
param (
    [String[]]$ChangedFiles = @(),

    [ValidateSet('true', 'false')]
    [String]$ForceCheckPowershell = $false,

    [ValidateSet('true', 'false')]
    [String]$ForceCheckDotnet = $false,

    [ValidateSet('true', 'false')]
    [String]$ForceCheckDocs = $false,

    [ValidateSet('true', 'false')]
    [String]$ForceBuildExternalDocs = $false,

    [ValidateSet('true', 'false')]
    [String]$ForcePublishDotnetPackages = $false
)

$ErrorActionPreference = "Stop"

function private:Set-OutputFlag([String]$Name, [Boolean]$Value) {
    Add-Content -Path $env:GITHUB_OUTPUT -Value "$Name=$Value"
}

Set-OutputFlag -Name 'check_powershell' -Value $(
    $ForceCheckPowershell -eq $true -or $ChangedFiles -match '^scripts/'
)

Set-OutputFlag -Name 'check_dotnet' -Value $(
    $ForceCheckDotnet -eq $true -or $ChangedFiles -match '^(src|tests)/'
)

Set-OutputFlag -Name 'check_docs_templates' -Value $(
    $ForceCheckDocs -eq $true -or $ChangedFiles -match '^docs/templates/'
)

Set-OutputFlag -Name 'build_external_docs' -Value $(
    $ForceBuildExternalDocs -eq $true -or $ChangedFiles -match '^docs/(external|namespaces|templates)/'
)

Set-OutputFlag -Name 'publish_dotnet_packages' -Value $(
    $ForcePublishDotnetPackages -eq $true -or $ChangedFiles -match $(@(
            "^src/Dfe.SignIn.Core.Framework/"
            "^src/Dfe.SignIn.Core.ExternalModels/"
            "^src/Dfe.SignIn.PublicApi.Client/"
            "^src/Dfe.SignIn.PublicApi.Client.AspNetCore/"
        ) -join '|')
)
