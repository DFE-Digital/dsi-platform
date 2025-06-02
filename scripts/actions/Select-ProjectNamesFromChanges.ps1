<#
.SYNOPSIS
    Select deployable .NET project names from changed files.

.OUTPUTS
    System.String[]. The list of selected names.

.EXAMPLE
    $projectNames = ./scripts/actions/Select-ProjectNamesFromChanges
#>
[CmdletBinding()]
param ()

$ErrorActionPreference = "Stop"

$changedFiles = ./scripts/actions/Discover-ChangedFilesInBranch
$projects = ./scripts/projects/Get-ProjectNamesFromFiles -Path "." -Files $changedFiles

return $projects.DeployableProjects
