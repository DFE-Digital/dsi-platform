<#
.SYNOPSIS
Writes project names to output.

.PARAMETER projectNames
The name project names information.

.EXAMPLE
$changedFiles = Discover-ChangedFilesInBranch
$changes = Get-ProjectNames ".." $changedFiles
Write-ProjectNamesToOutput $changes
#>
function Write-ProjectNamesToOutput([PSCustomObject]$changes) {
    Write-Output ""
    Write-Output "Source projects:"
    Write-Output "-------------------------"
    Write-Output $changes.SourceProjects
    Write-Output ""
    Write-Output "Run test projects:"
    Write-Output "-------------------------"
    Write-Output $changes.TestProjects
    Write-Output ""
    Write-Output "Could be deployed:"
    Write-Output "-------------------------"
    Write-Output $changes.DeployableProjects
    Write-Output ""
    Write-Output "Should publish NuGet?"
    Write-Output "-------------------------"
    Write-Output $changes.PublishPackages
}
