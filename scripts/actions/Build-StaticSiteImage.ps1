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
    [string]$HtmlPath
)

$ErrorActionPreference = "Stop"

$tag = "{0}/{1}:{2}-{3}-{4}" -f $RegistryServer, $($repositoryName.ToLower()), $Lifecycle, $RunId, $RunNumber

# az acr login --name $RegistryServer
docker build -t $tag --file ./docker/static-site/Dockerfile --build-arg HTML_PATH=$HtmlPath .
# docker push "$RegistryServer/$repositoryName" --all-tags
