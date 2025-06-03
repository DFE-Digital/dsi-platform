$docsPath = Resolve-Path "${PSScriptRoot}/../../docs"

Copy-Item -Path "$docsPath/api" -Destination "$docsPath/external" -recurse -Force
Copy-Item -Path "$docsPath/ruby" -Destination "$docsPath/external" -recurse -Force
