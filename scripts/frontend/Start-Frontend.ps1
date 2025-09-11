<#
.SYNOPSIS
    Build and start localhost server to preview frontend assets.

.DESCRIPTION
    This includes scripts and styles for GOV.UK design system components along with
    the various custom components that are used throughout the DfE Sign-in platform.

        http://localhost:8081

.OUTPUTS
    Output from building the frontend assets preview site.

.EXAMPLE
    ./scripts/frontend/Preview-Frontend
#>
[CmdletBinding()]
param ()

$ErrorActionPreference = "Stop"

$site = @{
    port = 8081
    tag  = 'frontend'
}

$root = Resolve-Path "$PSScriptRoot/../.."

docker build $root -f "$root/docker/frontend/Dockerfile" -t $($site.tag) `
    --build-arg PROJECT_NAME=$($site.path)

docker run -d -p "$($site.port):8080" `
    -e _ $($site.tag)
