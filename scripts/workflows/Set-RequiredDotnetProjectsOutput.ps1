<#
.SYNOPSIS
    Set workflow step outputs to specify which .NET projects need to be built.

.PARAMETER LifecycleName
    The lifecycle:
        - 'dev'
        - 'rel'

.PARAMETER ChangedFiles
    The list of zero-or-more changed files.

.PARAMETER IncludeChangedProjects
    A value indicating if .NET projects should be built when associated files have
    been changed. This is determined by checking the 'ChangedFiles' parameter.

.PARAMETER ForceBuildAuthExtensions
    A value indicating whether the 'Dfe.SignIn.Fn.AuthExtensions' .NET project
    should be built regardless of whether any associated files have been changed.

.PARAMETER ForceBuildSelectOrganisation
    A value indicating whether the 'Dfe.SignIn.Web.SelectOrganisation' .NET project
    should be built regardless of whether any associated files have been changed.

.PARAMETER ForceBuildPublicApi
    A value indicating whether the 'Dfe.SignIn.PublicApi' .NET project
    should be built regardless of whether any associated files have been changed.

.OUTPUTS
    GitHub outputs:
        - dotnet_images - A JSON encoded array of projects that need to be built:

          The following properties are available for each entry:
            - Project - The name of the project; for example, 'Dfe.SignIn.PublicApi'.
            - Repository - The name of the associated container repository; for example, 'public-api'.

.EXAMPLE
    $changedFiles = ./scripts/workflows/Get-ChangedFilesInBranch
    ./scripts/workflows/Set-RequiredDotnetProjectsOutput `
        -LifecycleName dev `
        -ChangedFiles $changedFiles `
        -IncludeChangedProjects true

.EXAMPLE
    ./scripts/workflows/Set-RequiredDotnetProjectsOutput `
        -LifecycleName dev `
        -ForceBuildSelectOrganisation true
#>
[CmdletBinding()]
[Diagnostics.CodeAnalysis.SuppressMessageAttribute(
    'PSReviewUnusedParameter', '',
    Justification = "This is a false positive; the parameter 'ChangedFiles' is being used."
)]
param (
    [Parameter(Mandatory = $true)]
    [ValidateSet('dev', 'rel', IgnoreCase = $false)]
    [String]$LifecycleName,

    [String[]]$ChangedFiles = @(),

    [ValidateSet('true', 'false')]
    [String]$IncludeChangedProjects = $false,

    [ValidateSet('true', 'false')]
    [String]$ForceBuildAuthExtensions = $false,

    [ValidateSet('true', 'false')]
    [String]$ForceBuildSelectOrganisation = $false,

    [ValidateSet('true', 'false')]
    [String]$ForceBuildPublicApi = $false
)

$ErrorActionPreference = "Stop"

$projectNames = @(
    if ($ForceBuildAuthExtensions -eq $true) {
        'Dfe.SignIn.Fn.AuthExtensions'
    }
    if ($ForceBuildSelectOrganisation -eq $true) {
        'Dfe.SignIn.Web.SelectOrganisation'
    }
    if ($ForceBuildPublicApi -eq $true) {
        'Dfe.SignIn.PublicApi'
    }
)

if ($IncludeChangedProjects -eq $true) {
    $projectNames = Invoke-Command {
        $projects = ./scripts/projects/Get-ProjectNamesFromFiles -Path . -Files $ChangedFiles
        return $projects.DeployableProjects
    }
}

$repositoryMappings = @{
    'Dfe.SignIn.Fn.AuthExtensions'      = @{
        Repository = 'auth-extensions'
        Dockerfile = 'functions'
    }
    'Dfe.SignIn.Web.SelectOrganisation' = @{
        Repository = 'select-organisation'
        Dockerfile = 'dotnet'
    }
    'Dfe.SignIn.PublicApi'              = @{
        Repository = 'public-api'
        Dockerfile = 'dotnet'
    }
}

$dotnetImages = $projectNames | Sort-Object | ForEach-Object {
    $mapping = $repositoryMappings[$_]
    if ($mapping) {
        @{
            project    = $_
            dockerfile = $mapping.Dockerfile
            repository = "$LifecycleName/$($mapping.Repository)".ToLower()
        }
    }
}

$json = ConvertTo-Json @($dotnetImages) -Compress
Write-Output "Projects: $( ConvertTo-Json @($dotnetImages) )"
Add-Content -Path $env:GITHUB_OUTPUT -Value "dotnet_images=$json"
