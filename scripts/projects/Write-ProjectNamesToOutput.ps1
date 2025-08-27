<#
.SYNOPSIS
    Writes project names to output.

.PARAMETER Projects
    The name project names information.

.OUTPUTS
    The collated project names.

.EXAMPLE
    $projects = ./scripts/projects/Get-ProjectNames -Path "."
    ./scripts/projects/Write-ProjectNamesToOutput -Projects $projects
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [PSCustomObject]$Projects
)

Write-Output "`nSource projects:"
Write-Output "-----------------------------"
Write-Output $Projects.SourceProjects | Sort-Object

Write-Output "`nTest projects:"
Write-Output "-----------------------------"
Write-Output $Projects.TestProjects | Sort-Object

Write-Output "`nDeployable projects:"
Write-Output "-----------------------------"
Write-Output $Projects.DeployableProjects | Sort-Object
