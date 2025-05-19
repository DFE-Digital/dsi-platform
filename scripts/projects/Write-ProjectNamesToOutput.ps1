<#
.SYNOPSIS
Writes project names to output.

.PARAMETER projects
The name project names information.

.EXAMPLE
$changedFiles = Discover-ChangedFilesInBranch
$projects = Get-ProjectNames ".." $changedFiles
Write-ProjectNamesToOutput $projects
#>
function Write-ProjectNamesToOutput {
    param(
        [PSCustomObject]$projects
    )

    Write-Output "`nSource projects:"
    Write-Output "-----------------------------"
    Write-Output $projects.SourceProjects

    Write-Output "`nTest projects:"
    Write-Output "-----------------------------"
    Write-Output $projects.TestProjects

    Write-Output "`nDeployable projects:"
    Write-Output "-----------------------------"
    Write-Output $projects.DeployableProjects

    Write-Output "`nHas publishable packages?"
    Write-Output "-----------------------------"
    Write-Output $projects.PublishPackages
}
