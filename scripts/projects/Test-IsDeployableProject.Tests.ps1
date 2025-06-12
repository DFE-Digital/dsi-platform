BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1', '.ps1')
}

Describe "Test-IsDeployableProject" {
    Context "when the project is a web-based project" {
        It "should return true for Dfe.SignIn.Web.Project" {
            $result = & $Cmdlet -ProjectName "Dfe.SignIn.Web.Project"
            $result | Should -Be $true
        }
    }

    Context "when the project is an API" {
        It "should return true for Dfe.SignIn.PublicApi" {
            $result = & $Cmdlet -ProjectName "Dfe.SignIn.PublicApi"
            $result | Should -Be $true
        }

        It "should return true for Dfe.SignIn.InternalApi" {
            $result = & $Cmdlet -ProjectName "Dfe.SignIn.InternalApi"
            $result | Should -Be $true
        }
    }

    Context "when the project is neither web-based nor an API" {
        It "should return false for Dfe.SignIn.OtherProject" {
            $result = & $Cmdlet -ProjectName "Dfe.SignIn.Core.ExternalModels"
            $result | Should -Be $false
        }

        It "should return false for Random.Project" {
            $result = & $Cmdlet -ProjectName "Random.Project"
            $result | Should -Be $false
        }
    }
}
