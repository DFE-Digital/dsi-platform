<#
.SYNOPSIS
    Build nginx docker image for a static site.

.PARAMETER RegistryServer
    The container registry server.

.PARAMETER RepositoryName
    The container repository name.

.PARAMETER ImageName
    Identifies the name of the image that is to be built.

.PARAMETER HtmlPath
    Path of the static site content. This is where the static .html files reside.

.OUTPUTS
    Feedback from docker image creation.

.EXAMPLE
    ./scripts/workflows/Build-StaticSiteImage `
        -RegistryServer ... `
        -RepositoryName 'select-organisation' `
        -ImageName 'dev-12345-1' `
        -HtmlPath './docs/_site'
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$RegistryServer,

    [Parameter(Mandatory=$true)]
    [string]$RepositoryName,

    [Parameter(Mandatory=$true)]
    [string]$ImageName,

    [Parameter(Mandatory=$true)]
    [string]$HtmlPath
)

$ErrorActionPreference = "Stop"

$tag = "{0}/{1}:{2}" -f $RegistryServer, $RepositoryName, $ImageName

# az acr login --name $RegistryServer
docker build -t $tag --file ./docker/static-site/Dockerfile --build-arg HTML_PATH=$HtmlPath .
# docker push "$RegistryServer/$repositoryName" --all-tags
