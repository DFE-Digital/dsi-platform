function Write-ProjectNamesToOutput {
<#
    .SYNOPSIS
        Writes project names to output.

    .PARAMETER Projects
        The name project names information.

    .EXAMPLE
        $changedFiles = Discover-ChangedFilesInBranch
        $projects = Get-ProjectNames ".." $changedFiles
        Write-ProjectNamesToOutput -Projects $projects
#>
    param (
        [Parameter(Mandatory=$true)]
        [PSCustomObject]$Projects
    )

    Write-Output "`nSource projects:"
    Write-Output "-----------------------------"
    Write-Output $Projects.SourceProjects

    Write-Output "`nTest projects:"
    Write-Output "-----------------------------"
    Write-Output $Projects.TestProjects

    Write-Output "`nDeployable projects:"
    Write-Output "-----------------------------"
    Write-Output $Projects.DeployableProjects

    Write-Output "`nHas publishable packages?"
    Write-Output "-----------------------------"
    Write-Output $Projects.PublishPackages
}
