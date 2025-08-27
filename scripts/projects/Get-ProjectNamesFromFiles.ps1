<#
.SYNOPSIS
    Gets the list of project names that would be affected by changes to any of the
    files that have been specified.

.PARAMETER Path
    Path to the solution directory.

.PARAMETER Files
    A list of filenames relative to the root directory of the git repository.

.OUTPUTS
    PSCustomObject. The function returns a custom object with the following properties:
        - SourceProjects: The list of affected source project names.
        - TestProjects: The list of associated test project names.
        - DeployableProjects: The list of project names that can be deployed.

.EXAMPLE
    $changedFiles = ./scripts/workflows/Get-ChangedFilesInBranch
    $projects = ./scripts/projects/Get-ProjectNamesFromFiles -Path "." -Files $changedFiles
    ./scripts/projects/Write-ProjectNamesToOutput -Projects $projects
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [String]$Path,

    [Parameter(Mandatory = $true)]
    [AllowEmptyCollection()]
    [String[]]$Files
)

$ErrorActionPreference = "Stop"

$Path = Resolve-Path $Path


#----- SourceProjects -------------------------------------------------------------------

# Extract the list of source project names where changes have been made.
$sourceProjects = $Files | ForEach-Object {
    if ($_ -match "^src/([^\\/]+)") { $matches[1] }
}

# Find projects that are dependant on the changed projects.
$allSourceProjects = Get-ChildItem -Directory "$Path/src" | Select-Object -ExpandProperty Name | Sort-Object
foreach ($projectName in $allSourceProjects) {
    $referencedProjects = dotnet list "$Path/src/$projectName/$projectName.csproj" reference | ForEach-Object {
        if ($_ -match "([^\\/]+)\.csproj") { $matches[1] }
    }
    foreach ($reference in $referencedProjects) {
        if ($sourceProjects -contains $reference) {
            $sourceProjects = @($sourceProjects) + $projectName
        }
    }
}

$sourceProjects = $sourceProjects | Sort-Object -Unique


#----- TestProjects ---------------------------------------------------------------------

# Get test projects that are associated with source projects.
$testProjects = $sourceProjects | ForEach-Object {
    $testProjectPath = Join-Path -Path $Path -ChildPath "tests/$_.UnitTests/$_.UnitTests.csproj"
    if (Test-Path $testProjectPath) {
        "$_.UnitTests"
    }
}

# Include test projects that were directly changed.
foreach ($file in $Files) {
    if ($file -match "^tests/([^\\/]+)") {
        $projectName = $matches[1]
        $testProjects = @($testProjects) + $projectName
        if ($projectName -notmatch "\.UnitTests$") {
            $testProjects = @($testProjects) + "$projectName.UnitTests"
        }
    }
}

# If there is at least one test project, ensure that test helpers are included.
if ($testProjects.Count -ne 0) {
    $testProjects = @($testProjects) + "Dfe.SignIn.TestHelpers"
}

$testProjects = $testProjects | Sort-Object -Unique


#----- DeployableProjects ---------------------------------------------------------------

# Extract the list of deployable project names.
$deployableProjects = $sourceProjects | ForEach-Object {
    $isDeployable = & "$PSScriptRoot/Test-IsDeployableProject.ps1" -ProjectName $_
    if ($isDeployable) { $_ }
}


#----- Result ---------------------------------------------------------------------------

return @{
    SourceProjects     = $sourceProjects
    TestProjects       = $testProjects
    DeployableProjects = $deployableProjects
}
