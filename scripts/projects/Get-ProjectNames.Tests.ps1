BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1', '.ps1')
}

Describe "Get-ProjectNames" {
    BeforeAll {
        Mock Get-ChildItem -ParameterFilter { $Path -ceq "./src" } {
            return @(
                @{ Name = "Dfe.SignIn.Core.Framework" }
                @{ Name = "Dfe.SignIn.Core.UseCases" }
                @{ Name = "Dfe.SignIn.PublicApi" }
                @{ Name = "Dfe.SignIn.PublicApi.Client" }
                @{ Name = "Dfe.SignIn.Web.SelectOrganisation" }
                @{ Name = "Dfe.SignIn.WebFramework" }
            )
        }
        Mock Get-ChildItem -ParameterFilter { $Path -ceq "./tests" } {
            return @(
                @{ Name = "Dfe.SignIn.Core.Framework.UnitTests" }
                @{ Name = "Dfe.SignIn.Core.UseCases.UnitTests" }
                @{ Name = "Dfe.SignIn.PublicApi.UnitTests" }
                @{ Name = "Dfe.SignIn.PublicApi.Client.UnitTests" }
                @{ Name = "Dfe.SignIn.Web.SelectOrganisation.UnitTests" }
                @{ Name = "Dfe.SignIn.WebFramework.UnitTests" }
                @{ Name = "Dfe.SignIn.TestHelpers" }
                @{ Name = "Dfe.SignIn.TestHelpers.UnitTests" }
            )
        }
        Mock Resolve-Path {
            return "."
        }
    }

    It "should set 'SourceProjects' to the list of associated source projects" {
        $result = & $Cmdlet -Path "."
        $result.SourceProjects | Should -Be @(
            "Dfe.SignIn.Core.Framework"
            "Dfe.SignIn.Core.UseCases"
            "Dfe.SignIn.PublicApi"
            "Dfe.SignIn.PublicApi.Client"
            "Dfe.SignIn.Web.SelectOrganisation"
            "Dfe.SignIn.WebFramework"
        )
    }

    It "should set 'TestProjects' to the list of all associated test projects" {
        $result = & $Cmdlet -Path "."
        $result.TestProjects | Should -Be @(
            "Dfe.SignIn.Core.Framework.UnitTests"
            "Dfe.SignIn.Core.UseCases.UnitTests"
            "Dfe.SignIn.PublicApi.Client.UnitTests"
            "Dfe.SignIn.PublicApi.UnitTests"
            "Dfe.SignIn.TestHelpers"
            "Dfe.SignIn.TestHelpers.UnitTests"
            "Dfe.SignIn.Web.SelectOrganisation.UnitTests"
            "Dfe.SignIn.WebFramework.UnitTests"
        )
    }

    It "should set 'DeployableProjects' to the list of deployable source projects" {
        $result = & $Cmdlet -Path "."
        $result.DeployableProjects | Should -Be @(
            "Dfe.SignIn.PublicApi"
            "Dfe.SignIn.Web.SelectOrganisation"
        )
    }
}
