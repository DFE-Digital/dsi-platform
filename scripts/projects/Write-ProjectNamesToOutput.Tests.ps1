BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1','.ps1')
}

Describe "Write-ProjectNamesToOutput" {
    Context "when no projects are specified" {
        It "outputs empty headings" {
            $output = & $Cmdlet -Projects @{}
            $output | Out-String | Should -Be @"

Source projects:
-----------------------------

Test projects:
-----------------------------

Deployable projects:
-----------------------------

"@
        }
    }

    Context "when source projects are specified" {
        It "outputs sorted listing of source projects" {
            $output = & $Cmdlet -Projects @{
                SourceProjects = @(
                    'Dfe.SignIn.Core.PublicApi'
                    'Dfe.SignIn.Core.PublicApi.Client'
                    'Dfe.SignIn.Core.Framework'
                )
            }
            $output | Out-String | Should -Be @"

Source projects:
-----------------------------
Dfe.SignIn.Core.Framework
Dfe.SignIn.Core.PublicApi
Dfe.SignIn.Core.PublicApi.Client

Test projects:
-----------------------------

Deployable projects:
-----------------------------

"@
        }
    }

    Context "when test projects are specified" {
        It "outputs sorted listing of test projects" {
            $output = & $Cmdlet -Projects @{
                TestProjects = @(
                    'Dfe.SignIn.PublicApi.UnitTests'
                    'Dfe.SignIn.PublicApi.Client.UnitTests'
                    'Dfe.SignIn.Core.Framework.UnitTests'
                )
            }
            $output | Out-String | Should -Be @"

Source projects:
-----------------------------

Test projects:
-----------------------------
Dfe.SignIn.Core.Framework.UnitTests
Dfe.SignIn.PublicApi.Client.UnitTests
Dfe.SignIn.PublicApi.UnitTests

Deployable projects:
-----------------------------

"@
        }
    }

    Context "when deployable projects are specified" {
        It "outputs sorted listing of deployable projects" {
            $output = & $Cmdlet -Projects @{
                DeployableProjects = @(
                    'Dfe.SignIn.Web.SelectOrganisation'
                    'Dfe.SignIn.PublicApi'
                )
            }
            $output | Out-String | Should -Be @"

Source projects:
-----------------------------

Test projects:
-----------------------------

Deployable projects:
-----------------------------
Dfe.SignIn.PublicApi
Dfe.SignIn.Web.SelectOrganisation

"@
        }
    }
}
