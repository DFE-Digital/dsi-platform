$docsPath = Resolve-Path "${PSScriptRoot}/../../docs"

. "${PSScriptRoot}/Build-CustomTemplate.ps1"

dotnet docfx "$docsPath/templates/Dfe.SignIn.DocfxPlugin.Tests/sample-docs/docfx.json" --serve --port 8087
