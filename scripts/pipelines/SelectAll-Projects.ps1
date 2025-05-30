[CmdletBinding()]
param ()

$ErrorActionPreference = "Stop"

. "${PSScriptRoot}/../projects/Get-ProjectNames.ps1"
. "${PSScriptRoot}/../projects/Write-ProjectNamesToOutput.ps1"
. "${PSScriptRoot}/Create-BuildSolution.ps1"

$projects = Get-ProjectNames -Path "."
Write-ProjectNamesToOutput -Projects $projects
Create-BuildSolution -Path "." -Projects $projects
