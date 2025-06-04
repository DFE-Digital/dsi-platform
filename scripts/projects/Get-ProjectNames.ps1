<#
.SYNOPSIS
    Gets the list of all source project names.

.PARAMETER Path
    Path to the solution directory.

.OUTPUTS
    System.Management.Automation.PSCustomObject. The function returns a custom object
    with the following properties:
        - SourceProjects: The list of affected source project names.
        - TestProjects: The list of associated test project names.
        - DeployableProjects: The list of project names that can be deployed.

.EXAMPLE
    $projects = ./scripts/projects/Get-ProjectNames -Path "."
    ./scripts/projects/Write-ProjectNamesToOutput -Projects $projects
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [String]$Path
)

$ErrorActionPreference = "Stop"

$Path = Resolve-Path $Path


#----- SourceProjects -------------------------------------------------------------------

$sourceProjects = Get-ChildItem -Directory "$Path/src" | Select-Object -ExpandProperty Name | Sort-Object


#----- TestProjects ---------------------------------------------------------------------

$testProjects = Get-ChildItem -Directory "$Path/tests" | Select-Object -ExpandProperty Name | Sort-Object


#----- DeployableProjects ---------------------------------------------------------------

# Extract the list of deployable project names.
$deployableProjects = $sourceProjects | ForEach-Object {
    $isDeployable = & "$PSScriptRoot/Test-DeployableProject.ps1" -ProjectName $_
    if ($isDeployable) { $_ }
}


#----- Result ---------------------------------------------------------------------------

return @{
    SourceProjects = $sourceProjects
    TestProjects = $testProjects
    DeployableProjects = $deployableProjects
}
