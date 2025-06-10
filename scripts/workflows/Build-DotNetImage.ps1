<#
.SYNOPSIS
    Build docker image for a .NET project.

.NOTES
    The repository name is determined from the given .NET project name.

.PARAMETER RegistryServer
    The container registry server.

.PARAMETER DotNetProjectName
    The name of the .NET project.

.PARAMETER ImageName
    Identifies the name of the image that is to be built.

.OUTPUTS
    Feedback from docker image creation.

.EXAMPLE
    ./scripts/workflows/Build-DotNetImage `
        -RegistryServer ... `
        -DotNetProjectName 'Dfe.SignIn.PublicApi' `
        -ImageName 'dev-12345-1'
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$RegistryServer,

    [Parameter(Mandatory=$true)]
    [string]$DotNetProjectName,

    [Parameter(Mandatory=$true)]
    [string]$ImageName
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

$tag = "{0}/{1}:{2}" -f $RegistryServer, $($repositoryName.ToLower()), $ImageName

# az acr login --name $RegistryServer
docker build -t $tag --file ./docker/dotnet-component/Dockerfile --build-arg PROC_FILE_NAME=$DotnetProjectName .
# docker push "$RegistryServer/$repositoryName" --all-tags
