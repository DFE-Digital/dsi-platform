<#
.SYNOPSIS
    Stop localhost server that is previewing frontend assets.

.PARAMETER Tag
    Tag of the docker image that needs to be stopped.

.EXAMPLE
    ./scripts/Stop-Docker -Tag frontend
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [String]$Tag
)

$ErrorActionPreference = "Stop"

docker ps --filter "ancestor=$Tag" | ForEach-Object {
    if (-not $_.StartsWith("CONTAINER")) {
        $processId = ($_ -split '\s+')[0]
        docker stop $processId
    }
}
