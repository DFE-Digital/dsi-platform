<#
.SYNOPSIS
    Build and start localhost server to preview developer reference documentation site.

.DESCRIPTION
    The developer reference documentation site includes content that is automatically
    generated from XML comments throughout the .NET projects.

    The "external" variation of the site includes guidance but only includes reference
    documentation for projects which are published as NuGet packages for external use
    by relying parties.

        http://localhost:8085

    The "internal" variation of the site includes reference documentation for all
    .NET projects.

        http://localhost:8086

    The "test" variation of the site is the site used for snapshot testing.

        http://localhost:8087

.OUTPUTS
    Output from building the external developer reference documentation site.

.EXAMPLE
    ./scripts/docs/Start-Site external

.EXAMPLE
    ./scripts/docs/Start-Site internal

.EXAMPLE
    ./scripts/docs/Start-Site test
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [ValidateSet('external', 'internal', 'test', IgnoreCase = $false)]
    [String]$Name
)

$ErrorActionPreference = "Stop"

$site = @{
    external = @{
        port = 8085
        path = 'docs/external'
        tag  = 'external-docs'
    }
    internal = @{
        port = 8086
        path = 'docs/internal'
        tag  = 'internal-docs'
    }
    test     = @{
        port = 8087
        path = 'docs/templates/Dfe.SignIn.DocfxPlugin.Tests/sample-docs'
        tag  = 'test-docs'
    }
}[$Name]

$root = Resolve-Path "$PSScriptRoot/../.."

docker build $root -f "$root/docker/docs/Dockerfile" -t $($site.tag) `
    --build-arg PROJECT_NAME=$($site.path) `
    --build-arg CDN_BASE_ADDRESS_FORMAT=%s%.s

docker run -d -p "$($site.port):8080" `
    -e CDN_BASE_ADDRESS=http://localhost:8081/ `
    -e SURVEY_URL=https://survey.localhost/ `
    -e _ $($site.tag)
