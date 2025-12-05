<#
.SYNOPSIS
    Set workflow step output flags to specify which checks need to be performed.

.PARAMETER LifecycleName
    The lifecycle:
        - 'dev'
        - 'rel'

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

.PARAMETER ForcePublishDotnetPackages
    A value indicating whether NuGet packages need to be built and published regardless of
    whether any related package source files have been changed.

.PARAMETER BuildChangedProjects
    A value indicating if .NET projects should be built when associated files have
    been changed. This is determined by checking the 'ChangedFiles' parameter.

.PARAMETER BuildForRepository
    When specified, indicates that an image should be built for the named repository
    regardless of whether any associated changes have been detected.

.OUTPUTS
    GitHub outputs:
        - check_powershell - 'true' for PowerShell checks.
        - check_dotnet - 'true' for .NET checks.
        - check_docs_templates - 'true' for documentation checks.
        - publish_dotnet_packages - 'true' to publish NuGet packages.
        - dotnet_images - A JSON encoded array of projects that need to be built.
          The following properties are available for each entry:
            - Project - The name of the project; for example, 'Dfe.SignIn.PublicApi'.
            - Repository - The name of the associated container repository; for example, 'public-api'.

.EXAMPLE
    $changedFiles = ./scripts/workflows/Get-ChangedFilesInBranch
    ./scripts/workflows/Set-RequirementsOutput `
        -LifecycleName dev `
        -ChangedFiles $changedFiles

.EXAMPLE
    $changedFiles = ./scripts/workflows/Get-ChangedFilesInBranch
    ./scripts/workflows/Set-RequirementsOutput `
        -LifecycleName dev `
        -ChangedFiles $changedFiles `
        -ForceCheckPowershell true

.EXAMPLE
    ./scripts/workflows/Set-RequirementsOutput `
        -LifecycleName dev `
        -BuildForRepository public-api
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [ValidateSet('dev', 'rel', IgnoreCase = $false)]
    [String]$LifecycleName,

    [String[]]$ChangedFiles = @(),

    [ValidateSet('true', 'false')]
    [String]$ForceCheckPowershell = $false,

    [ValidateSet('true', 'false')]
    [String]$ForceCheckDotnet = $false,

    [ValidateSet('true', 'false')]
    [String]$ForceCheckDocs = $false,

    [ValidateSet('true', 'false')]
    [String]$BuildChangedProjects = $false,

    [String]$BuildForRepository = $null,

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

Set-OutputFlag -Name 'publish_dotnet_packages' -Value $(
    $ForcePublishDotnetPackages -eq $true -or $ChangedFiles -match $(@(
            "^src/Dfe.SignIn.Base.Framework/"
            "^src/Dfe.SignIn.Core.Public/"
            "^src/Dfe.SignIn.PublicApi.Client/"
            "^src/Dfe.SignIn.PublicApi.Client.AspNetCore/"
        ) -join '|')
)

$repositoryMappings = @{
    'frontend'                          = @{
        Repository = 'frontend'
        Dockerfile = 'frontend'
        Version    = (Get-Content './frontend/package.json' | ConvertFrom-Json).version
    }
    'Dfe.SignIn.Fn.AuthExtensions'      = @{
        Repository = 'auth-extensions'
        Dockerfile = 'functions'
    }
    'Dfe.SignIn.InternalApi'            = @{
        Repository = 'internal-api'
        Dockerfile = 'dotnet'
    }
    'Dfe.SignIn.PublicApi'              = @{
        Repository = 'public-api'
        Dockerfile = 'dotnet'
    }
    'Dfe.SignIn.Web.Profile'            = @{
        Repository = 'profile'
        Dockerfile = 'dotnet'
    }
    'Dfe.SignIn.Web.Help'               = @{
        Repository = 'help'
        Dockerfile = 'dotnet'
    }
    'Dfe.SignIn.Web.SelectOrganisation' = @{
        Repository = 'select-organisation'
        Dockerfile = 'dotnet'
    }
    'docs/external'                     = @{
        Repository = 'developer'
        Dockerfile = 'docs'
    }
}

$buildProjectNames = [System.Collections.Generic.HashSet[string]]::new()

if ($BuildForRepository -eq '(all components)') {
    # Force all projects to be built.
    foreach ($mapping in $repositoryMappings.GetEnumerator()) {
        $buildProjectNames.Add($mapping.Key)
    }
}
elseif ($BuildForRepository -and $BuildForRepository -ne '(n/a)') {
    # Force a specific project to be built.
    $matchedMapping = $repositoryMappings.GetEnumerator() | `
        Where-Object { $_.Value.Repository -ceq $BuildForRepository } | `
        Select-Object -First 1
    if (-not $matchedMapping) {
        throw "Invalid repository name '$BuildForRepository'."
    }
    $buildProjectNames.Add($matchedMapping.Key)
}

# Include the names of projects relating to files that have been changed?
if ($BuildChangedProjects -eq $true) {
    if ($ChangedFiles -match '^docs/(external|namespaces|templates)/') {
        $buildProjectNames.Add('docs/external')
    }

    if ($ChangedFiles -match '^frontend/') {
        $buildProjectNames.Add('frontend')
    }

    $projectNamesFromFiles += Invoke-Command {
        $projects = ./scripts/projects/Get-ProjectNamesFromFiles -Path . -Files $ChangedFiles
        return $projects.DeployableProjects
    }
    foreach ($projectName in $projectNamesFromFiles) {
        $buildProjectNames.Add($projectName)
    }
}

# Map to output values for workflow.
$dockerImages = @($buildProjectNames) | Sort-Object | ForEach-Object {
    $mapping = $repositoryMappings[$_]
    if ($mapping) {
        $imageInfo = @{
            project    = $_
            dockerfile = $mapping.Dockerfile
            repository = "$LifecycleName/$($mapping.Repository)".ToLower()
        }
        if ($mapping.Version) {
            $imageInfo.version = $mapping.Version
        }
        return $imageInfo
    }
}

$json = ConvertTo-Json @($dockerImages) -Compress
Write-Output "Projects: $( ConvertTo-Json @($dockerImages) )"
Add-Content -Path $env:GITHUB_OUTPUT -Value "docker_images=$json"
