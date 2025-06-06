<#
.SYNOPSIS
    Build and start localhost server to preview the internal generated docs site.

.NOTES
    The internal site includes documentation that is automatically generated from XML
    comments throughout the repository.

    This version of the documentation does not include all guidance information.
    See the 'Preview-ExternalSite' script to preview external content.

    Preview runs locally on: http://localhost:8086

.OUTPUTS
    Output from building the internal developer reference documentation site.

    Includes a link to the localhost preview server.

.EXAMPLE
    ./scripts/docs/Preview-InternalSite
#>
[CmdletBinding()]
param ()

$ErrorActionPreference = "Stop"

$docsPath = Resolve-Path "${PSScriptRoot}/../../docs"

Invoke-Expression "${PSScriptRoot}/Build-CustomTemplate.ps1"

dotnet docfx "$docsPath/internal/docfx.json" --serve --port 8086
