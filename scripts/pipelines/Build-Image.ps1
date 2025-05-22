[CmdletBinding()]
param(
    [string]$regServer,
    [string]$appName,
    [string]$targetWorkflow,
    [string]$buildId,
    [string]$dockerFileName,
    [string]$dotNetProjName = ""
)

$tag = "{0}/{1}:{2}-{3}" -f $regServer,$($appName.ToLower()),$($targetWorkflow.ToLower()),$buildId

az acr login --name $regServer
docker build -t $tag --file "./docker/Linux-app-service/$dockerFileName.dockerfile" --build-arg PROC_FILE_NAME=$dotNetProjName . 
docker push "$regServer/$($appName.ToLower())" --all-tags
