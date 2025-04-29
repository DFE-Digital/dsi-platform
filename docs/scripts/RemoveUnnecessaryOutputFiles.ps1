Remove-Item `
    ../_site/index.json, `
    ../_site/manifest.json, `
    ../_site/toc.json, `
    ../_site/xrefmap.yml, `
    ../_site/styles `
    -Force -Recurse `
    -ErrorAction SilentlyContinue
