[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$RegServer,

    [Parameter(Mandatory=$true)]
    [string]$AppName,

    [Parameter(Mandatory=$true)]
    [string]$Lifecycle,

    [Parameter(Mandatory=$true)]
    [string]$BuildId,

    [Parameter(Mandatory=$true)]
    [string]$DockerFileName,

    [string]$DotNetProjName = ""
)

$ErrorActionPreference = "Stop"

$tag = "{0}/{1}:{2}-{3}" -f $RegServer,$($AppName.ToLower()),$($Lifecycle.ToLower()),$BuildId

az acr login --name $RegServer
docker build -t $tag --file "./docker/Linux-app-service/$DockerFileName.dockerfile" --build-arg PROC_FILE_NAME=$DotNetProjName . 
docker push "$RegServer/$($AppName.ToLower())" --all-tags
