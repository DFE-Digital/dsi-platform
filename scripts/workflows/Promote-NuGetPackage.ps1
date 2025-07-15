<#
.SYNOPSIS
    Promote a NuGet package in Azure Artifacts feed to either @Release or @Prerelease view.

.DESCRIPTION
    This script uploads a NuGet package and sets its view in Azure Artifacts
    according to the lifecycle stage ('dev' or 'rel').
    'rel' corresponds to @Release view and 'dev' corresponds to @Prerelease view.

.PARAMETER PackagePath
    The full path to the NuGet package (.nupkg file) to promote.

.PARAMETER LifecycleStage
    The lifecycle stage of the package.
    Allowed values are 'dev' or 'rel'. Determines which view to assign (@Prerelease or @Release).

.PARAMETER Pat
    The Azure DevOps Personal Access Token (PAT) used for authentication.

.PARAMETER BaseFeedUrl
    The base URL of the Azure Artifacts NuGet feed for promoting packages.
    Example: https://feeds.dev.azure.com/myOrg/myProject/_apis/packaging/feeds/myFeed/nuget/packages/

.EXAMPLE
    ./Promote-NuGetPackage.ps1 `
      -PackagePath "/packages/MyPackage.1.0.0.nupkg" `
      -LifecycleStage "rel" `
      -Pat "myPATtoken" `
      -BaseFeedUrl "https://feeds.dev.azure.com/myOrg/myProject/_apis/packaging/feeds/myFeed/nuget/packages/"
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $PackagePath,

    [Parameter(Mandatory)]
    [string] $LifecycleStage,

    [Parameter(Mandatory)]
    [string] $Pat,

    [Parameter(Mandatory)]
    [string] $BaseFeedUrl
)

$ErrorActionPreference = "Stop"

# Extract package id and version from the filename
# /home/runner/work/dsi-platform/dsi-platform/src/Dfe.SignIn.PublicApi.Client.AspNetCore/bin/Release/Dfe.SignIn.PublicApi.Client.AspNetCore.1.0.9326.23534.nupkg
# Dfe.SignIn.PublicApi.Client.AspNetCore.1.0.9326.23534
$filename = [System.IO.Path]::GetFileNameWithoutExtension($PackagePath)

# The file version is the last 4 parts separated by dot
# Dfe.SignIn.PublicApi.Client.AspNetCore.1.0.9326.23534
# 1.0.9326.23534
$parts = $filename -split '\.'
$versionParts = $parts[-4..-1]
$version = $versionParts -join '.'
$packageId = $parts[0..($parts.Length - 5)] -join '.'

$view = if ($LifecycleStage -eq 'rel') { 'Release' } else { 'Prerelease' }

if ($baseFeedUrl.EndsWith('/')) {
    $baseFeedUrl = $baseFeedUrl.Substring(0, $baseFeedUrl.Length - 1)
}

$promotionUrl = "${baseFeedUrl}/${packageId}/versions/${version}?api-version=7.1"

$authHeader = @{
    Authorization  = ("Basic {0}" -f [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$Pat")))
    "Content-Type" = "application/json"
}

$body = @{
    views = @{
        op    = "add"
        path  = "/views/-"
        value = "${view}"
    }
} | ConvertTo-Json -Depth 3

# Do HTTP Patch request to promote package
$response = Invoke-RestMethod -Uri $promotionUrl -Method Patch -Headers $authHeader -Body $body
Write-Host "Package '$PackageId' version '$Version' promoted to view '$view'."

return $response
