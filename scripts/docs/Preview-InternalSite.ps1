$docsPath = Resolve-Path "${PSScriptRoot}/../../docs"

. "${PSScriptRoot}/Build-CustomTemplate.ps1"
. "${PSScriptRoot}/Prepare-InternalSite.ps1"

dotnet docfx "$docsPath/internal/docfx.json" --serve --port 8086
