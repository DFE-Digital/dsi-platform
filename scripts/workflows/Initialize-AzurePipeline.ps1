<#
.SYNOPSIS
    Triggers an Azure DevOps pipeline run and returns the resulting run URL.

.DESCRIPTION
    Authenticates using a Personal Access Token (PAT), constructs a request body with pipeline parameters and the desired Git ref,
    and calls the Azure DevOps REST API to start a pipeline run. Outputs the run URL as a GitHub Actions output variable.

.PARAMETER Pat
    The Azure DevOps Personal Access Token (PAT) used for authentication.

.PARAMETER OrgProjectUrl
    The full Azure DevOps organization/project URL (e.g., https://dev.azure.com/org/project).

.PARAMETER PipelineId
    The ID of the pipeline to run.

.PARAMETER BranchRef
    The full Git reference of the branch to build (e.g., refs/heads/main).

.PARAMETER ProjectName
    The project name to pass to the pipeline as a template parameter.

.PARAMETER RepositoryName
    The container repository name to pass to the pipeline.

.PARAMETER Tag
    The image tag to associate with this run.

.PARAMETER TransformationEnv
    A boolean flag to indicate whether this is a transformation environment.

.OUTPUTS
    Writes the pipeline run URL to the GitHub Actions output via `GITHUB_OUTPUT`.

.EXAMPLE
    ./scripts/Trigger-Pipeline.ps1 `
        -Pat "abc123..." `
        -OrgProjectUrl "https://dev.azure.com/my-org/project" `
        -PipelineId "42" `
        -BranchRef "refs/heads/feature/new-feature" `
        -ProjectName "MyApp" `
        -RepositoryName "myapp/repo" `
        -Tag "build-123" `
        -TransformationEnv $True
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string] $Pat,

    [Parameter(Mandatory = $true)]
    [string] $OrgProjectUrl,

    [Parameter(Mandatory = $true)]
    [string] $PipelineId,

    [Parameter(Mandatory = $true)]
    [string] $BranchRef,

    [Parameter(Mandatory = $true)]
    [string] $ProjectName,

    [Parameter(Mandatory = $true)]
    [string] $RepositoryName,

    [Parameter(Mandatory = $true)]
    [string] $Tag,

    [Parameter(Mandatory = $true)]
    [bool] $TransformationEnv
)

$ErrorActionPreference = "Stop"

$authBytes  = [Text.Encoding]::ASCII.GetBytes(":$Pat")
$base64Auth = [Convert]::ToBase64String($authBytes)
$headers = @{
    "Authorization" = "Basic $base64Auth"
    "Content-Type"  = "application/json"
}

$body = @{
  resources = @{
    repositories = @{
      self = @{
        refName = $BranchRef
      }
    }
  }
  templateParameters = @{
      projectName = $ProjectName
      repositoryName = $RepositoryName
      tag = $Tag
      tran = $TransformationEnv
  }
} | ConvertTo-Json -Depth 5

$url = "$OrgProjectUrl/_apis/pipelines/$PipelineId/runs?api-version=7.1-preview.1"

$response = Invoke-RestMethod -Method Post -Uri $url -Headers $headers -Body $body

return $response.url

