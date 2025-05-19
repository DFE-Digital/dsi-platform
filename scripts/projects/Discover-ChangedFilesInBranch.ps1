<#
.SYNOPSIS
Discovers all of the files that have changed in the active git branch.

.EXAMPLE
$changedFiles = Discover-ChangedFilesInBranch

.OUTPUTS
System.String[]. The function returns an array of paths that are relative to the root
directory of the git repository.
#>
function Discover-ChangedFilesInBranch {
    git fetch origin main:refs/remotes/origin/main
    return git diff --name-only HEAD origin/main
}
