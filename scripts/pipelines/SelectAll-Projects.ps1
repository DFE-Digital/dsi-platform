. "${PSScriptRoot}/../projects/Get-ProjectNames.ps1"
. "${PSScriptRoot}/../projects/Write-ProjectNamesToOutput.ps1"
. "${PSScriptRoot}/Create-BuildSolution.ps1"

$projects = Get-ProjectNames "."
Write-ProjectNamesToOutput $projects
Create-BuildSolution "." $projects
