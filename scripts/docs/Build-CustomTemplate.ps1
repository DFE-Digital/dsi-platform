$docsPath = Resolve-Path "${PSScriptRoot}/../../docs"

pwsh -WorkingDirectory "$docsPath/templates" -Command {
    npm install
    npm run build
}

dotnet build "$docsPath/templates"
