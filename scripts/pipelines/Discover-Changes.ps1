[CmdletBinding()]
param ()

$ErrorActionPreference = "Stop"

. "${PSScriptRoot}/../projects/Discover-ChangedFilesInBranch.ps1"
. "${PSScriptRoot}/../projects/Get-ProjectNamesFromFiles.ps1"
. "${PSScriptRoot}/../projects/Write-ProjectNamesToOutput.ps1"
. "${PSScriptRoot}/Create-BuildSolution.ps1"

$changedFiles = Discover-ChangedFilesInBranch
$projects = Get-ProjectNamesFromFiles -Path "." -Files $changedFiles
Write-ProjectNamesToOutput -Projects $projects
Create-BuildSolution -Path "." -Projects $projects

Write-Host "##vso[task.setvariable variable=publishPackages]$($projects.PublishPackages)"
Write-Host "##vso[task.setvariable variable=deployProjectNames]$($projects.DeployableProjects -join ';');"
