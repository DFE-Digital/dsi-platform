$docsPath = Resolve-Path "${PSScriptRoot}/../../docs"

. "${PSScriptRoot}/Build-CustomTemplate.ps1"

dotnet docfx "$docsPath/external/docfx.json" --serve --port 8085
