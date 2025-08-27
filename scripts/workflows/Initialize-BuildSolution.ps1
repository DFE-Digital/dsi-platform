<#
.SYNOPSIS
    Create 'build.sln' for the provided projects.

.PARAMETER Projects
    The name project names information:
        - SourceProjects - an array of source project names.
        - TestProjects - an array of test project names.

.OUTPUTS
    Solution creation feedback.

.EXAMPLE
    ./scripts/workflows/Initialize-BuildSolution `
        -Path "." `
        -Projects $projects

.EXAMPLE
    ./scripts/workflows/Initialize-BuildSolution `
        -Path "." `
        -Projects @{
            SourceProjects = @( 'Dfe.SignIn.PublicApi' )
            TestProjects = @( 'Dfe.SignIn.PublicApi.UnitTests' )
        }
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [PSCustomObject]$Projects
)

$ErrorActionPreference = "Stop"

$path = Resolve-Path "."

Write-Host "`nCreating build.sln..."
dotnet new sln --name build --output $path

Write-Host "Adding projects..."
$sourceProjectPaths = $Projects.SourceProjects | ForEach-Object {
    if ($_ -ne $null) {
        Join-Path -Path $path -ChildPath "src/${_}/${_}.csproj"
    }
}
$testProjectPaths = $Projects.TestProjects | ForEach-Object {
    if ($_ -ne $null) {
        Join-Path -Path $path -ChildPath "tests/${_}/${_}.csproj"
    }
}

$projectPaths = @() + $sourceProjectPaths + $testProjectPaths
$projectPaths = $projectPaths.Where({ Test-Path $_ })

if ($projectPaths.Count -gt 0) {
    dotnet sln $path/build.sln add $projectPaths
}
