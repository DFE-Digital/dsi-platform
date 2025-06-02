[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$RegistryServer,

    [Parameter(Mandatory=$true)]
    [string]$Lifecycle,

    [Parameter(Mandatory=$true)]
    [string]$RunId,

    [Parameter(Mandatory=$true)]
    [string]$RunNumber,

    [Parameter(Mandatory=$true)]
    [string]$DotNetProjectName
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
