$docsPath = Resolve-Path "${PSScriptRoot}/../../docs"

Remove-Item `
    "$docsPath/_site/index.json", `
    "$docsPath/_site/manifest.json", `
    "$docsPath/_site/toc.json", `
    "$docsPath/_site/xrefmap.yml", `
    "$docsPath/_site/styles" `
    -Force -Recurse `
    -ErrorAction SilentlyContinue
