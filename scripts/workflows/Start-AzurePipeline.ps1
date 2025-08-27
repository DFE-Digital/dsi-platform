<#
.SYNOPSIS
    Starts an Azure DevOps pipeline.

.DESCRIPTION
    Authenticates using an access token, constructs a request body with pipeline
    parameters and the desired Git ref, and calls the Azure DevOps REST API to start
    a pipeline run. Outputs the run URL as a GitHub Actions output variable.

.PARAMETER Token
    The token for authorizing with Azure DevOps.

.PARAMETER ProjectUrl
    The full Azure DevOps organization/project URL (e.g., https://dev.azure.com/org/project).

.PARAMETER PipelineId
    The ID of the pipeline to run.

.PARAMETER TemplateParameters
    An object with key/value template parameters.

.OUTPUTS
    Response data from starting Azure pipeline.

.EXAMPLE
    ./scripts/workflows/Start-AzurePipeline `
        -Token "abc123..." `
        -ProjectUrl "https://dev.azure.com/my-org/project" `
        -PipelineId "42" `
        -TemplateParameters @{
            projectName = "Dfe.SignIn.Web.Help"
            repositoryName = "dev/help"
            tag = "123456-1"
            tran = $True
        }
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string] $Token,

    [Parameter(Mandatory = $true)]
    [string] $ProjectUrl,

    [Parameter(Mandatory = $true)]
    [string] $PipelineId,

    [Parameter(Mandatory = $true)]
    [PSCustomObject] $TemplateParameters
)

$ErrorActionPreference = "Stop"

$authBytes = [Text.Encoding]::ASCII.GetBytes(":$Token")
$base64Auth = [Convert]::ToBase64String($authBytes)
$headers = @{
    "Authorization" = "Basic $base64Auth"
    "Content-Type"  = "application/json"
}

$body = @{
    templateParameters = $TemplateParameters
} | ConvertTo-Json -Depth 5

$url = "$ProjectUrl/_apis/pipelines/$PipelineId/runs?api-version=7.1"

$response = Invoke-RestMethod -Method Post -Uri $url -Headers $headers -Body $body

return $response
