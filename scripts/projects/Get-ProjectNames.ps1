. "${PSScriptRoot}/Is-DeployableProject.ps1"

<#
.SYNOPSIS
Gets the list of all source project names.

.PARAMETER path
Path to the solution directory.

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
        [string]$path
    )

    $path = Resolve-Path $path

    $sourceProjects = Get-ChildItem -Directory "$path/src" | Select-Object -ExpandProperty Name | Sort-Object
    $testProjects = Get-ChildItem -Directory "$path/tests" | Select-Object -ExpandProperty Name | Sort-Object

    # Are any of the source projects NuGet packages?
    $publishPackages = $false
    foreach ($projectName in $sourceProjects) {
        $csprojContent = Get-Content -Path "$path/src/$projectName/$projectName.csproj"
        if ($csprojContent -match "<IsPackable>true</IsPackable>") {
            $publishPackages = $true
            break
        }
    }

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
