<#
.SYNOPSIS
    Build and start localhost server to preview the template test site.

.NOTES
    The test project can be found inside the docs/templates/Dfe.SignIn.DocfxPlugin.Tests
    path which is used for automated snapshot testing.

    This command makes it easy for developers to visually review the template test site.

    Preview runs locally on: http://localhost:8087

.OUTPUTS
    Output from building the template test site.

    Includes a link to the localhost preview server.

.EXAMPLE
    ./scripts/docs/Preview-TestSite
#>
[CmdletBinding()]
param ()

$ErrorActionPreference = "Stop"

$docsPath = Resolve-Path "${PSScriptRoot}/../../docs"

Invoke-Expression "${PSScriptRoot}/Build-CustomTemplate.ps1"

dotnet docfx "$docsPath/templates/Dfe.SignIn.DocfxPlugin.Tests/sample-docs/docfx.json" --serve --port 8087
