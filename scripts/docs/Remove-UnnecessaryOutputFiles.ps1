<#
.SYNOPSIS
    Remove unnecessary files from generated developer reference site.

.NOTES
    During the website generation process there are extra unwanted files in the output
    directory which can be removed from the final distribution:
        - Intermediate files such as 'toc.json' and 'index.json which are used during
          the post-processing step to generate navigation menus and search index.
        - Unused files such as 'manifest.json' and 'xrefmap.yml'.

.PARAMETER OutputPath
    Path to the site output path.

.OUTPUTS
    None.

.EXAMPLE
    ./scripts/docs/Remove-UnnecessaryOutputFiles -OutputPath ./docs/_site
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [String]$OutputPath
)

$ErrorActionPreference = "Stop"

$docsPath = Resolve-Path $OutputPath

Write-Host $docsPath

Remove-Item `
    "$docsPath/index.json", `
    "$docsPath/manifest.json", `
    "$docsPath/**/toc.json", `
    "$docsPath/xrefmap.yml", `
    -Force `
    -ErrorAction SilentlyContinue
