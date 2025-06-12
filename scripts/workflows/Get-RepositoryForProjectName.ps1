<#
.SYNOPSIS
    Gets the container registry repository for a given project name.

.PARAMETER ProjectName
    The name of the project.

.PARAMETER LifecycleName
    The name of the lifecycle:
        - 'dev'
        - 'rel'

.OUTPUTS
    String. The resolved repository name.

    Outputs $null when no repository mapping is available.

.EXAMPLE
    ./scripts/workflows/Get-RepositoryForProjectName `
        -ProjectName 'Dfe.SignIn.PublicApi' `
        -LifecycleName 'dev'
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string]$ProjectName,

    [Parameter(Mandatory = $true)]
    [string]$LifecycleName
)

$ErrorActionPreference = "Stop"

if ($LifecycleName -notmatch "^(dev|rel)$") {
    throw "Invalid lifecycle '$LifecycleName'."
}

$imageMappings = @{
    'Dfe.SignIn.Web.SelectOrganisation' = 'select-organisation'
    'Dfe.SignIn.PublicApi'              = 'public-api-v2'
}

$repositoryName = $imageMappings[$ProjectName]
if (-not $repositoryName) {
    return $null
}

if ($LifecycleName -eq 'rel') {
    $repositoryName = "$repositoryName-$LifecycleName"
}

return $repositoryName
