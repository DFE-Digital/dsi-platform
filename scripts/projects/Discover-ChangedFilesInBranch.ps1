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

    $baseCommitRef = git rev-parse origin/main
    $headCommitRef = git rev-parse HEAD

    # HEAD will be the same when merging into the main branch.
    if ($baseCommitRef -eq $headCommitRef) {
        # Compare with the previous merge.
        $baseCommitRef = git log origin/main~1 --merges -n 1 --pretty=format:%H
    }

    $changedFiles = git diff --name-only HEAD $baseCommitRef
    return $changedFiles
}
