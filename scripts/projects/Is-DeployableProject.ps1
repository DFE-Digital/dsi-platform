function Is-DeployableProject {
<#
    .SYNOPSIS
        Identifies whether the specified project can be deployed.

    .NOTES
        Projects can only be deployed if they fit one of the following criteria:
            - The project is a web-based project; eg. Dfe.SignIn.Web.*
            - The project is an API; eg. Dfe.SignIn.*Api

    .PARAMETER ProjectName
        The name of the project; for example, "Dfe.SignIn.PublicApi".

    .OUTPUTS
        System.Boolean. The function returns $true if the project is deployable,
        otherwise false.

    .EXAMPLE
        if (Is-DeployableProject("Dfe.SignIn.PublicApi")) {
            Write-Content "The project is deployable!"
        }
#>
    param (
        [Parameter(Mandatory=$true)]
        [string]$ProjectName
    )

    return $ProjectName -match "^Dfe\.SignIn\.(Web\.|[^.]+Api$)"
}
