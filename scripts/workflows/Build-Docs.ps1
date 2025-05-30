<#
.SYNOPSIS
    Build DocFX developer reference documentation from markdown files and XML comments.

.NOTES
    Generated website is output to the path './docs/_site'.

    Any tooling must be setup before this script is ran.

.PARAMETER Audience
    The target audience of the generated documentation:
    - 'External' - Intended for use by relying parties to help with integration.
    - 'Internal' - Intended for use by DfE Sign-in team members.

.OUTPUTS
    Feedback from docker image creation.

.EXAMPLE
    dotnet tool restore
    ./scripts/workflows/Build-Docs -Audience 'External'
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$Audience
)

$ErrorActionPreference = "Stop"

if (@('External', 'Internal') -notcontains $Audience) {
    throw "Invalid audience '$Audience'."
}

$Audience = $Audience.ToLower()

# Specify environment placeholders for use with Server Side Includes (SSI).
dotnet docfx ./docs/$Audience/docfx.json `
    -m _cdnBaseAddress='<!--#echo var="CDN_BASE_ADDRESS"-->' `
    -m _cdnVersion='<!--#echo var="CDN_VERSION"-->' `
    -m _surveyUrl='<!--#echo var="SURVEY_URL"-->'

Move-Item ./docs/$Audience/_site ./docs/_site
./scripts/docs/Remove-UnnecessaryOutputFiles -OutputPath ./docs/_site
