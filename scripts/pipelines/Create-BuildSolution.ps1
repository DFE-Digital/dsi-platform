<#
.SYNOPSIS
Create 'build.sln' for the provided projects.

.PARAMETER path
Path to the solution directory.

.PARAMETER projects
The name project names information.
#>
function Create-BuildSolution {
    param(
        [string]$path,
        [PSCustomObject]$projects
    )

    $path = Resolve-Path $path

    Write-Output "`nCreating build.sln..."
    dotnet new sln --name build --output $path

    Write-Output "Adding projects..."
    $sourceProjectPaths = @($projects.SourceProjects | ForEach-Object { Resolve-Path "$path/src/${_}/${_}.csproj" })
    $testProjectPaths = @($projects.TestProjects | ForEach-Object { Resolve-Path "$path/tests/${_}/${_}.csproj" })
    $projectPaths = @($sourceProjectPaths + $testProjectPaths)
    dotnet sln $path/build.sln add $projectPaths
}
