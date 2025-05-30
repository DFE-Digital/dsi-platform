<#
.SYNOPSIS
    Create 'build.sln' for the provided projects.

.PARAMETER Projects
    The name project names information.

.OUTPUTS
    Solution creation feedback.

.EXAMPLE
    ./scripts/workflows/Initialize-BuildSolution -Path "." -Projects $projects
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [PSCustomObject]$Projects
)

$ErrorActionPreference = "Stop"

$path = Resolve-Path "."

Write-Output "`nCreating build.sln..."
dotnet new sln --name build --output $path

Write-Output "Adding projects..."
$sourceProjectPaths = $Projects.SourceProjects | ForEach-Object { Resolve-Path "$path/src/${_}/${_}.csproj" }
$testProjectPaths = $Projects.TestProjects | ForEach-Object { Resolve-Path "$path/tests/${_}/${_}.csproj" }
$projectPaths = $sourceProjectPaths + $testProjectPaths
if ($projectPaths.Count -ne 0) {
    dotnet sln $path/build.sln add $projectPaths
}
