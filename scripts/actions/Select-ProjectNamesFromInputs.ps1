<#
.SYNOPSIS
    Select deployable .NET project names from inputs.

.OUTPUTS
    System.String[]. The list of selected names.

.EXAMPLE
    $projectNames = ./scripts/actions/Select-ProjectNamesFromInputs `
        -SelectOrganisation true `
        -BuildPublicApi false
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
