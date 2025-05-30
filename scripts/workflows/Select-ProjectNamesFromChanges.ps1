<#
.SYNOPSIS
    Select deployable .NET project names for the current changeset.

.NOTES
    The changeset is determine by the 'Discover-ChangedFilesInBranch' script.

.OUTPUTS
    String[]. The list of deployable .NET project names.

.EXAMPLE
    $projectNames = ./scripts/workflows/Select-ProjectNamesFromChanges
#>
[CmdletBinding()]
param ()

$ErrorActionPreference = "Stop"

$changedFiles = ./scripts/workflows/Discover-ChangedFilesInBranch
$projects = ./scripts/projects/Get-ProjectNamesFromFiles -Path "." -Files $changedFiles

return $projects.DeployableProjects
