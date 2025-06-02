<#
.SYNOPSIS
    Create 'build.sln' for projects that affected by the current changes.

.EXAMPLE
    ./scripts/actions/Initialize-BuildSolutionForChangedProjects
#>
[CmdletBinding()]
param ()

$ErrorActionPreference = "Stop"

$changedFiles = ./scripts/actions/Discover-ChangedFilesInBranch
$projects = ./scripts/projects/Get-ProjectNamesFromFiles -Path "." -Files $changedFiles
./scripts/projects/Write-ProjectNamesToOutput -Projects $projects
./scripts/actions/Initialize-BuildSolution -Projects $projects
