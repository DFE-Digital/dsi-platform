<#
.SYNOPSIS
    Select deployable projects by mapping workflow toggles to .NET project names.

.OUTPUTS
    String[]. The list of deployable .NET project names.

.EXAMPLE
    $projectNames = ./scripts/workflows/Get-ProjectNamesFromParameters `
        -SelectOrganisation true `
        -BuildPublicApi false

    # Outputs @( 'Dfe.SignIn.Web.SelectOrganisation' )
#>
[CmdletBinding()]
param (
    [String]$SelectOrganisation,
    [String]$PublicApi
)

$ErrorActionPreference = "Stop"

[String[]]$projectNames = @()

$mappings = @(
    @{
        ManualTrigger = $SelectOrganisation -eq 'true'
        ProjectName = 'Dfe.SignIn.Web.SelectOrganisation'
    }
    @{
        ManualTrigger = $PublicApi -eq 'true'
        ProjectName = 'Dfe.SignIn.PublicApi'
    }
)

return $mappings | ForEach-Object {
    if ($_.ManualTrigger) { return $_.ProjectName }
}
