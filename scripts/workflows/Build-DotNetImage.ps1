<#
.SYNOPSIS
    Build docker image for a .NET project.

.NOTES
    The repository name is determined from the given .NET project name.

.PARAMETER RegistryServer
    The container registry server.

.PARAMETER DotNetProjectName
    The name of the .NET project.

.PARAMETER Lifecycle
    Intended lifecycle of image:
        - 'dev' to indicate the development lifecycle.
        - 'rel' to indicate the release lifecycle.

.PARAMETER RunId
    Identifies the unique run of the GitHub workflow.

.PARAMETER RunNumber
    Identifies the unique run number of the GitHub workflow.

.OUTPUTS
    Feedback from docker image creation.

.EXAMPLE
    ./scripts/workflows/Build-DotNetImage `
        -RegistryServer ... `
        -DotNetProjectName 'Dfe.SignIn.PublicApi' `
        -Lifecycle 'dev' `
        -RunId 12345 `
        -RunNumber 1
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$RegistryServer,

    [Parameter(Mandatory=$true)]
    [string]$DotNetProjectName,

    [Parameter(Mandatory=$true)]
    [string]$Lifecycle,

    [Parameter(Mandatory=$true)]
    [string]$RunId,

    [Parameter(Mandatory=$true)]
    [string]$RunNumber
)

$ErrorActionPreference = "Stop"

$imageMappings = @{
    "Dfe.SignIn.Web.SelectOrganisation" = "select-organisation"
    "Dfe.SignIn.PublicApi" = "public-api-v2"
}

$repositoryName = $imageMappings[$DotNetProjectName]
if (-not $repositoryName) {
    throw "Repository mapping not specified for '$DotNetProjectName'."
}

$tag = "{0}/{1}:{2}-{3}-{4}" -f $RegistryServer, $($repositoryName.ToLower()), $Lifecycle, $RunId, $RunNumber

# az acr login --name $RegistryServer
docker build -t $tag --file ./docker/dotnet-component/Dockerfile --build-arg PROC_FILE_NAME=$DotnetProjectName .
# docker push "$RegistryServer/$repositoryName" --all-tags
