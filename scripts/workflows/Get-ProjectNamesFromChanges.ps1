<#
.SYNOPSIS
    Select deployable .NET project names for the current changeset.

.NOTES
    The changeset is determine by the 'Get-ChangedFilesInBranch' script.

.OUTPUTS
    String[]. The list of deployable .NET project names.

.EXAMPLE
    $projectNames = ./scripts/workflows/Get-ProjectNamesFromChanges
#>
[CmdletBinding()]
param ()

$ErrorActionPreference = "Stop"

$changedFiles = ./scripts/workflows/Get-ChangedFilesInBranch
$projects = ./scripts/projects/Get-ProjectNamesFromFiles -Path "." -Files $changedFiles

return $projects.DeployableProjects
