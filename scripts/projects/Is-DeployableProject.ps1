<#
.SYNOPSIS
Identifies whether the specified project can be deployed.

.DESCRIPTION
Projects can only be deployed if they fit one of the following criteria:
    - The project is a web-based project; eg. Dfe.SignIn.Web.*
    - The project is an API; eg. Dfe.SignIn.*Api

.PARAMETER projectName
The name of the project; for example, "Dfe.SignIn.PublicApi".

.EXAMPLE
if (Is-DeployableProject("Dfe.SignIn.PublicApi")) {
    Write-Content "The project is deployable!"
}

.OUTPUTS
System.Boolean. The function returns $true if the project is deployable,
otherwise false.
#>
function Is-DeployableProject([string]$projectName) {
    return $projectName -match "^Dfe\.SignIn\.(Web\.|[^.]+Api$)"
}
