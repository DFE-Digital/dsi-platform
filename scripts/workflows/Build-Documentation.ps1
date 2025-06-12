<#
.SYNOPSIS
    Build DocFX developer reference documentation from markdown files and XML comments.

.NOTES
    Generated website is output to the path './docs/_site'.

    Any tooling must be setup before this script is ran.

.PARAMETER ProjectName
    The documentation project name:
    - 'docs/external' - Intended for use by relying parties to help with integration.
    - 'docs/internal' - Intended for use by DfE Sign-in team members.

.OUTPUTS
    Feedback from docker image creation.

.EXAMPLE
    dotnet tool install docfx --local
    ./scripts/workflows/Build-Documentation -ProjectName docs/external
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string]$ProjectName
)

$ErrorActionPreference = "Stop"

if (@('docs/external', 'docs/internal') -notcontains $ProjectName) {
    throw "Invalid documentation project name '$ProjectName'."
}

# Specify environment placeholders for use with Server Side Includes (SSI).
dotnet docfx ./$ProjectName/docfx.json `
    -m _cdnBaseAddress='<!--#echo var="CDN_BASE_ADDRESS"-->' `
    -m _cdnVersion='<!--#echo var="CDN_VERSION"-->' `
    -m _surveyUrl='<!--#echo var="SURVEY_URL"-->'

Move-Item ./$ProjectName/_site ./docs/_site
./scripts/docs/Remove-UnnecessaryOutputFiles -OutputPath ./docs/_site
