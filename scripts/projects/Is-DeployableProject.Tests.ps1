BeforeAll {
    . $PSCommandPath.Replace('.Tests.ps1','.ps1')
}

Describe "Is-DeployableProject" {
    Context "when the project is a web-based project" {
        It "should return true for Dfe.SignIn.Web.Project" {
            $result = Is-DeployableProject -ProjectName "Dfe.SignIn.Web.Project"
            $result | Should -Be $true
        }
    }

    Context "when the project is an API" {
        It "should return true for Dfe.SignIn.PublicApi" {
            $result = Is-DeployableProject -ProjectName "Dfe.SignIn.PublicApi"
            $result | Should -Be $true
        }

        It "should return true for Dfe.SignIn.InternalApi" {
            $result = Is-DeployableProject -ProjectName "Dfe.SignIn.InternalApi"
            $result | Should -Be $true
        }
    }

    Context "when the project is neither web-based nor an API" {
        It "should return false for Dfe.SignIn.OtherProject" {
            $result = Is-DeployableProject -ProjectName "Dfe.SignIn.Core.ExternalModels"
            $result | Should -Be $false
        }

        It "should return false for Random.Project" {
            $result = Is-DeployableProject -ProjectName "Random.Project"
            $result | Should -Be $false
        }
    }
}
