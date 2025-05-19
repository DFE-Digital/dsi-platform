. "${PSScriptRoot}/Is-DeployableProject.ps1"

<#
.SYNOPSIS
Gets the list of project names that would be affected by changes to any of the files
that have been specified.

.PARAMETER path
Path to the solution directory.

.PARAMETER files
A list of filenames relative to the root directory of the git repository.

.OUTPUTS
System.Management.Automation.PSCustomObject. The function returns a custom object
with the following properties:
    - SourceProjects: The list of affected source project names.
    - TestProjects: The list of associated test project names.
    - DeployableProjects: The list of project names that can be deployed.
    - PublishPackages: A value of true indicates that project(s) could be published.
#>
function Get-ProjectNames {
    param(
        [string]$path,
        [string[]]$files
    )

    $path = Resolve-Path $path
    $files = @($files)

    # Extract the list of source project names where changes have been made.
    $sourceProjects = @($files | ForEach-Object {
        if ($_ -match "^src/([^\\/]+)") { $matches[1] }
    })

    $allSourceProjects = Get-ChildItem -Directory "$path/src" | Select-Object -ExpandProperty Name | Sort-Object
    foreach ($projectName in $allSourceProjects) {
        $referencedProjects = dotnet list "$path/src/$projectName/$projectName.csproj" reference | ForEach-Object {
            if ($_ -match "([^\\/]+)\.csproj") { $matches[1] }
        }
        foreach ($reference in $referencedProjects) {
            if ($sourceProjects -contains $reference) {
                $sourceProjects += $projectName
            }
        }
    }

    $sourceProjects = $sourceProjects | Sort-Object -Unique

    # Are any of the source projects NuGet packages?
    $publishPackages = $false
    foreach ($projectName in $sourceProjects) {
        $csprojContent = Get-Content -Path "$path/src/$projectName/$projectName.csproj"
        if ($csprojContent -match "<IsPackable>true</IsPackable>") {
            $publishPackages = $true
            break
        }
    }

    # Extract the list of relevant test project names.
    $associatedTestProjects = @($sourceProjects | ForEach-Object {
        "${_}.UnitTests"
    })
    $changedTestProjects = @($files | ForEach-Object {
        if ($_ -match "^tests/([^\\/]+.UnitTests)") { $matches[1] }
    })
    $testProjects = ($associatedTestProjects + $changedTestProjects) | Sort-Object -Unique

    # Extract the list of deployable project names.
    $deployableProjects = @($sourceProjects | ForEach-Object {
        if (Is-DeployableProject($_)) { $_ }
    })

    return @{
        SourceProjects = $sourceProjects
        TestProjects = $testProjects
        DeployableProjects = $deployableProjects
        PublishPackages = $publishPackages
    }
}
