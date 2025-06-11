<#
.SYNOPSIS
    Resolve lifecycle from an implied or overridden parameter.

.PARAMETER Implied
    The implied lifecycle:
        - 'dev'
        - 'rel'

.PARAMETER Override
    The lifecycle override:
        - 'Development'
        - 'Release'

.OUTPUTS
    String. A value of 'dev' if the development lifecycle is intended; otherwise, a value
    of 'rel' indicating that the release lifecycle is intended.

.EXAMPLE
    ./scripts/workflows/Resolve-LifecycleParameter `
        -Implied 'rel' `
        -Override ''
    # rel

.EXAMPLE
    ./scripts/workflows/Resolve-LifecycleParameter `
        -Implied 'rel' `
        -Override 'Development'
    # dev
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [String]$Implied,

    [String]$Override
)

$ErrorActionPreference = "Stop"

if ($Override.Length -ge 3) {
    $lifecycle = $Override.ToLower().Substring(0, 3)
}
else {
    $lifecycle = $Implied
}

if ($lifecycle -notmatch "^(dev|rel)$") {
    throw "Invalid lifecycle '$lifecycle'."
}

Write-Host "Resolved lifecycle (implied: '$Implied', override: '$Override') -> $lifecycle"

return $lifecycle
