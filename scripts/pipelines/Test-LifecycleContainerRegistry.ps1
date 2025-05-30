[CmdletBinding()]
param (
    [Parameter(Mandatory=$true)]
    [string]$Lifecycle,

    [Parameter(Mandatory=$true)]
    [string]$ContainerRegistry
)

$Lifecycle = $Lifecycle.ToLower()
$ContainerRegistry = $ContainerRegistry.ToLower()

if ($ContainerRegistry -notmatch "^(standard|d03)$") {
    throw "Unexpected container registry '$ContainerRegistry'."
}

switch ($Lifecycle) {
    "development" {
        break
    }
    "release" {
        if ($ContainerRegistry -eq "d03") {
            throw "Cannot use release lifecycle for an image from the D03 container registry."
        }
        break
    }
    Default {
        throw "Unexpected lifecycle '$Lifecycle'."
    }
}
