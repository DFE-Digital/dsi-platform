<#
.SYNOPSIS
    Verifies that the test snapshots meet expectations.

.DESCRIPTION
    Accepts the unverified snapshot files as the new snapshots.

    This replaces the previous snapshot files.

    This command should only be used once the snapshot diffs have been manually reviewed
    to ensure that only the expected changes have occurred.

.OUTPUTS
    None.

.EXAMPLE
    # Run unit tests for docfx templates.
    dotnet test docs/templates

    # Manually verify any unverified snapshots and then when satisfied use
    # the following command to accept the new snapshots.
    ./scripts/docs/Confirm-TestSnapshots
#>
[CmdletBinding()]
param ()

$ErrorActionPreference = "Stop"

$docsPath = Resolve-Path -Path "${PSScriptRoot}/../../docs"

Get-ChildItem -Path "$docsPath/templates/Dfe.SignIn.DocfxPlugin.Tests/snapshots/*.received.txt" | ForEach-Object {
    $verifiedName = $_.Name -replace '\.received\.txt$', '.verified.txt'
    $source = Join-Path -Path $_.DirectoryName -ChildPath $_.Name
    $destination = Join-Path -Path $_.DirectoryName -ChildPath $verifiedName
    Move-Item -Path $source -Destination $destination -Force
}
