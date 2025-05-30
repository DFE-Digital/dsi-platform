<#
.SYNOPSIS
    Build nginx docker image for a static site.

.PARAMETER RegistryServer
    The container registry server.

.PARAMETER RepositoryName
    The container repository name.

.PARAMETER Lifecycle
    Intended lifecycle of image:
        - 'dev' to indicate the development lifecycle.
        - 'rel' to indicate the release lifecycle.

.PARAMETER RunId
    Identifies the unique run of the GitHub workflow.

.PARAMETER RunNumber
    Identifies the unique run number of the GitHub workflow.

.PARAMETER HtmlPath
    Path of the static site content. This is where the static .html files reside.

.OUTPUTS
    Feedback from docker image creation.

.EXAMPLE
    ./scripts/workflows/Build-StaticSiteImage `
        -RegistryServer ... `
        -RepositoryName 'select-organisation' `
        -Lifecycle 'dev' `
        -RunId 12345 `
        -RunNumber 1 `
        -HtmlPath './docs/_site'
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$RegistryServer,

    [Parameter(Mandatory=$true)]
    [string]$RepositoryName,

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

$tag = "{0}/{1}:{2}-{3}-{4}" -f $RegistryServer, $RepositoryName, $Lifecycle, $RunId, $RunNumber

# az acr login --name $RegistryServer
docker build -t $tag --file ./docker/static-site/Dockerfile --build-arg HTML_PATH=$HtmlPath .
# docker push "$RegistryServer/$repositoryName" --all-tags
