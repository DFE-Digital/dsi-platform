$docsPath = Resolve-Path "${PSScriptRoot}/../../docs"

Copy-Item -Path "$docsPath/api" -Destination "$docsPath/internal" -recurse -Force
Copy-Item -Path "$docsPath/ruby" -Destination "$docsPath/internal" -recurse -Force
