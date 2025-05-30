<#
.SYNOPSIS
    Gets the list of project names that would be affected by changes to any of the
    files that have been specified.

.PARAMETER Path
    Path to the solution directory.

.PARAMETER Files
    A list of filenames relative to the root directory of the git repository.

.OUTPUTS
    System.Management.Automation.PSCustomObject. The function returns a custom object
    with the following properties:
        - SourceProjects: The list of affected source project names.
        - TestProjects: The list of associated test project names.
        - DeployableProjects: The list of project names that can be deployed.
        - PublishPackages: A value of true indicates that project(s) could be published.
#>
param (
    [Parameter(Mandatory=$true)]
    [String]$Path,

    [Parameter(Mandatory=$true)]
    [String[]]$Files
)

$ErrorActionPreference = "Stop"

$Path = Resolve-Path $Path

. "${PSScriptRoot}/Is-DeployableProject.ps1"

# Extract the list of source project names where changes have been made.
[String[]]$sourceProjects = $Files | ForEach-Object {
    if ($_ -match "^src/([^\\/]+)") { $matches[1] }
}

[String[]]$allSourceProjects = Get-ChildItem -Directory "$Path/src" | Select-Object -ExpandProperty Name | Sort-Object
foreach ($projectName in $allSourceProjects) {
    $referencedProjects = dotnet list "$Path/src/$projectName/$projectName.csproj" reference | ForEach-Object {
        if ($_ -match "([^\\/]+)\.csproj") { $matches[1] }
    }
    foreach ($reference in $referencedProjects) {
        if ($sourceProjects -contains $reference) {
            $sourceProjects += $projectName
        }
    }
}

[String[]]$sourceProjects = $sourceProjects | Sort-Object -Unique

# Extract the list of relevant test project names.
[String[]]$associatedTestProjects = $sourceProjects | ForEach-Object {
    "${_}.UnitTests"
}
[String[]]$changedTestProjects = $Files | ForEach-Object {
    if ($_ -match "^tests/") { $matches[1] }
}
[String[]]$testProjects = ($associatedTestProjects + $changedTestProjects) | Sort-Object -Unique
if ($testProjects.Count -ne 0) {
    $testProjects = $testProjects + @( "Dfe.SignIn.TestHelpers" )
}

# Are any of the source projects NuGet packages?
$publishPackages = $false
foreach ($projectName in $sourceProjects) {
    $csprojContent = Get-Content -Path "$Path/src/$projectName/$projectName.csproj"
    if ($csprojContent -match "<IsPackable>true</IsPackable>") {
        $publishPackages = $true
        break
    }
}

# Extract the list of deployable project names.
[String[]]$deployableProjects = $sourceProjects | ForEach-Object {
    if (Is-DeployableProject($_)) { $_ }
}

return @{
    SourceProjects = $sourceProjects
    TestProjects = $testProjects
    DeployableProjects = $deployableProjects
    PublishPackages = $publishPackages
}
