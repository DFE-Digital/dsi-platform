<#
.SYNOPSIS
    Create 'build.sln' for projects that affected by the current changes.

.NOTES
    The changeset is determine by the 'Get-ChangedFilesInBranch' script.

.OUTPUTS
    Summary of changed projects and solution creation feedback.

.EXAMPLE
    ./scripts/workflows/Initialize-BuildSolutionForChangedProjects
#>
[CmdletBinding()]
param ()

$ErrorActionPreference = "Stop"

$changedFiles = ./scripts/workflows/Get-ChangedFilesInBranch
$projects = ./scripts/projects/Get-ProjectNamesFromFiles -Path "." -Files $changedFiles
./scripts/projects/Write-ProjectNamesToOutput -Projects $projects
./scripts/workflows/Initialize-BuildSolution -Projects $projects
