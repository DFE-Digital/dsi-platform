<#
.SYNOPSIS
    Build and start localhost server to preview the external generated docs site.

.NOTES
    The external site includes documentation that is automatically generated from XML
    comments of projects that are published as NuGet packages for use by relying
    parties.

    This version of the documentation does not generate documentation for all projects.
    See the 'Preview-InternalSite' script to preview all generated XML content.

    Preview runs locally on: http://localhost:8085

.OUTPUTS
    Output from building the external developer reference documentation site.

    Includes a link to the localhost preview server.

.EXAMPLE
    ./scripts/docs/Preview-ExternalSite
#>
[CmdletBinding()]
param ()

$ErrorActionPreference = "Stop"

$docsPath = Resolve-Path "${PSScriptRoot}/../../docs"

Invoke-Expression "${PSScriptRoot}/Build-CustomTemplate.ps1"

dotnet docfx "$docsPath/external/docfx.json" --serve --port 8085
