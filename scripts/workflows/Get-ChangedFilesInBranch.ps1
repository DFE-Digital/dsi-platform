<#
.SYNOPSIS
    Discovers all of the files that have changed in the active git branch.

.OUTPUTS
    String[]. The function returns an array of paths that are relative to the
    root directory of the git repository.

.EXAMPLE
    $changedFiles = ./scripts/workflows/Get-ChangedFilesInBranch
#>
[CmdletBinding()]
param ()

$ErrorActionPreference = "Stop"

git fetch origin main:refs/remotes/origin/main

$baseCommitRef = git rev-parse origin/main
$headCommitRef = git rev-parse HEAD

[String[]] $changedFiles = git diff --name-only HEAD $baseCommitRef
return $changedFiles
