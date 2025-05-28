$docsPath = Resolve-Path "${PSScriptRoot}/../../docs"

Copy-Item -Path "$docsPath/api" -Destination "$docsPath/internal" -recurse -Force
