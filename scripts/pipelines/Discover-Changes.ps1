. "${PSScriptRoot}/../projects/Discover-ChangedFilesInBranch.ps1"
. "${PSScriptRoot}/../projects/Get-ProjectNamesFromFiles.ps1"
. "${PSScriptRoot}/../projects/Write-ProjectNamesToOutput.ps1"
. "${PSScriptRoot}/Create-BuildSolution.ps1"

$changedFiles = Discover-ChangedFilesInBranch
$projects = Get-ProjectNamesFromFiles "." $changedFiles
Write-ProjectNamesToOutput $projects
Create-BuildSolution "." $projects

Write-Host "##vso[task.setvariable variable=publishPackages]$($projects.PublishPackages)"
Write-Host "##vso[task.setvariable variable=deployProjectNames]$($projects.DeployableProjects -join ';');"
