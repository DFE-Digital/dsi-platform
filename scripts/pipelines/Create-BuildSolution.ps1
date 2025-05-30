function Create-BuildSolution {
<#
    .SYNOPSIS
        Create 'build.sln' for the provided projects.

    .PARAMETER Path
        Path to the solution directory.

    .PARAMETER Projects
        The name project names information.
#>
    param (
        [Parameter(Mandatory=$true)]
        [string]$Path,

        [Parameter(Mandatory=$true)]
        [PSCustomObject]$Projects
    )

    $ErrorActionPreference = "Stop"

    $Path = Resolve-Path $Path

    Write-Output "`nCreating build.sln..."
    dotnet new sln --name build --output $Path

    Write-Output "Adding projects..."
    $sourceProjectPaths = @($Projects.SourceProjects | ForEach-Object { Resolve-Path "$Path/src/${_}/${_}.csproj" })
    $testProjectPaths = @($Projects.TestProjects | ForEach-Object { Resolve-Path "$Path/tests/${_}/${_}.csproj" })
    $projectPaths = @($sourceProjectPaths + $testProjectPaths)
    dotnet sln $Path/build.sln add $projectPaths
}
