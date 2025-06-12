<#
.SYNOPSIS
    Build custom docfx templates.

.NOTES
    Scripts and styles are bundled for use in the browser (see './docs/templates').

    Builds post-processing plugins for docfx (see './docs/templates/Dfe.SignIn.DocfxPlugin').

.OUTPUTS
    None.

.EXAMPLE
    ./scripts/docs/Build-CustomTemplate
#>
[CmdletBinding()]
param ()

$ErrorActionPreference = "Stop"

$docsPath = Resolve-Path "${PSScriptRoot}/../../docs"

pwsh -WorkingDirectory "$docsPath/templates" -Command {
    npm install --ignore-scripts
    npm run build
}

dotnet build "$docsPath/templates"
