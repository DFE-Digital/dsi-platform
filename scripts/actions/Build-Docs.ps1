[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$Audience
)

$ErrorActionPreference = "Stop"

dotnet docfx ./docs/$($Audience.ToLower())/docfx.json `
    -m _cdnBaseAddress='<!--#echo var="CDN_BASE_ADDRESS"-->' `
    -m _cdnVersion='<!--#echo var="CDN_VERSION"-->' `
    -m _surveyUrl='<!--#echo var="SURVEY_URL"-->'

Move-Item ./docs/external/_site ./docs/_site
./scripts/docs/Remove-UnnecessaryOutputFiles
