$docsPath = Resolve-Path "${PSScriptRoot}/../../docs"

npm --prefix "$docsPath/templates" install
npm --prefix "$docsPath/templates" run build
dotnet build "$docsPath/templates"
