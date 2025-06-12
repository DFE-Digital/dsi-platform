BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1', '.ps1')
}

Describe "Get-ProjectNamesFromParameters" {
    Context "when no inputs are given" {
        It "should return no project names" {
            $result = & $Cmdlet

            $result | Should -Be @()
        }
    }

    Context "when '<InputName>' input is given" -ForEach @(
        @{
            InputName           = 'SelectOrganisation'
            ExpectedProjectName = 'Dfe.SignIn.Web.SelectOrganisation'
        }
    ) {
        It "should return the expected project name when input is 'true'" {
            $params = @{ $InputName = 'true' }
            $result = & $Cmdlet @params

            $result | Should -Be @( $ExpectedProjectName )
        }

        It "should not return the expected project name when input is 'false'" {
            $params = @{ $InputName = 'false' }
            $result = & $Cmdlet @params

            $result | Should -Be @()
        }
    }

    Context "when multiple inputs are given" {
        It "should return the expected project names when some are true" {
            $result = & $Cmdlet `
                -SelectOrganisation 'true' `
                -PublicApi 'false'

            $result | Should -Be @( 'Dfe.SignIn.Web.SelectOrganisation' )
        }

        It "should return the expected project names when all are true" {
            $result = & $Cmdlet `
                -SelectOrganisation 'true' `
                -PublicApi 'true'

            $result | Sort-Object | Should -Be @(
                'Dfe.SignIn.PublicApi'
                'Dfe.SignIn.Web.SelectOrganisation'
            )
        }
    }
}
