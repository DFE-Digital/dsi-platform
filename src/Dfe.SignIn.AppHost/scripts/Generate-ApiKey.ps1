<#
.SYNOPSIS
    Stub: generate a local development API key.

.DESCRIPTION
    Invoked from the Aspire dashboard via WithProcessCommand on Internal/Public API.
    Replace this stub with real key generation / persistence when ready.
    Stdout is shown in the Aspire command result dialog.

.PARAMETER ClientName
    Name of the client the key is for.

.PARAMETER ApiKind
    Target API: internal or public.

.PARAMETER Description
    Optional description for the key.

.PARAMETER Environment
    Target environment name.

.PARAMETER ExpiresDays
    Number of days until the key expires.

.PARAMETER CreateInactive
    When true, create the key in an inactive state.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $ClientName,

    [Parameter(Mandatory = $true)]
    [ValidateSet('internal', 'public')]
    [string] $ApiKind,

    [Parameter(Mandatory = $false)]
    [string] $Description = '',

    [Parameter(Mandatory = $true)]
    [string] $Environment,

    [Parameter(Mandatory = $true)]
    [int] $ExpiresDays,

    [Parameter(Mandatory = $false)]
    [string] $CreateInactive = 'false'
)

$createInactiveBool = $CreateInactive -eq 'true' -or $CreateInactive -eq 'True' -or $CreateInactive -eq '1'

$bytes = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
$apiKey = [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')

$expiresOn = (Get-Date).ToUniversalTime().AddDays($ExpiresDays).ToString('yyyy-MM-dd')
$status = if ($createInactiveBool) { 'Inactive' } else { 'Active' }

Write-Output "API: $ApiKind"
Write-Output "Client: $ClientName"
Write-Output "Environment: $Environment"
Write-Output "Description: $(if ($Description) { $Description } else { '(none)' })"
Write-Output "Expires on: $expiresOn ($ExpiresDays days)"
Write-Output "Status: $status"
Write-Output "API key: $apiKey"
Write-Output ""
Write-Output "(Stub — not persisted. Replace scripts/Generate-ApiKey.ps1 when ready.)"
