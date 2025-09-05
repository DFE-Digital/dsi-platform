BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1', '.ps1')
}

Describe "Get-ProjectNamesFromFiles" {
    BeforeAll {
        $srcProjectPaths = @(
            @{ Name = "Dfe.SignIn.Base.Framework" }
            @{ Name = "Dfe.SignIn.Core.UseCases" }
            @{ Name = "Dfe.SignIn.PublicApi.Client" }
            @{ Name = "Dfe.SignIn.PublicApi" }
            @{ Name = "Dfe.SignIn.Web.SelectOrganisation" }
            @{ Name = "Dfe.SignIn.WebFramework" }
        )

        $testProjectPaths = @(
            @{ Name = "Dfe.SignIn.Base.Framework.UnitTests" }
            @{ Name = "Dfe.SignIn.Core.UseCases.UnitTests" }
            @{ Name = "Dfe.SignIn.PublicApi.Client.UnitTests" }
            @{ Name = "Dfe.SignIn.PublicApi.UnitTests" }
            @{ Name = "Dfe.SignIn.TestHelpers.UnitTests" }
            @{ Name = "Dfe.SignIn.TestHelpers" }
            @{ Name = "Dfe.SignIn.Web.SelectOrganisation.UnitTests" }
            @{ Name = "Dfe.SignIn.WebFramework.UnitTests" }
        )

        Mock Get-ChildItem -ParameterFilter { $Path -ceq "./src" } {
            return $srcProjectPaths
        }
        Mock Get-ChildItem -ParameterFilter { $Path -ceq "./tests" } {
            return $testProjectPaths
        }
        Mock Resolve-Path {
            return "."
        }
        Mock Get-Content {
            return "<Project></Project>"
        }
        Mock Test-Path {
            return $true
        }
        Mock dotnet -ParameterFilter { $args[0] -ceq "list" } {
            return @()
        }
    }

    Context "when no files have been changed" {
        BeforeEach {
            $changedFiles = @()
        }

        It "should not fail" {
            & $Cmdlet -Path "." -Files $changedFiles
        }
    }

    Context "when source files have been changed" {
        BeforeEach {
            $changedFiles = @(
                "src/Dfe.SignIn.Base.Framework/SomeFile.cs"
                "src/Dfe.SignIn.Base.Framework/AnotherFile.cs"
                "src/Dfe.SignIn.WebFramework/Helpers.cs"
                "README.md"
            )
        }

        It "should set 'SourceProjects' to the list of associated source projects" {
            $result = & $Cmdlet -Path "." -Files $changedFiles

            $result.SourceProjects | Should -Be @(
                "Dfe.SignIn.Base.Framework"
                "Dfe.SignIn.WebFramework"
            )
        }

        It "should set 'TestProjects' to the list of all associated test projects" {
            $result = & $Cmdlet -Path "." -Files $changedFiles

            $result.TestProjects | Should -Be @(
                "Dfe.SignIn.Base.Framework.UnitTests"
                "Dfe.SignIn.TestHelpers"
                "Dfe.SignIn.WebFramework.UnitTests"
            )
        }
    }

    Context "when source files have been changed which affect dependants" {
        BeforeEach {
            $changedFiles = @(
                "src/Dfe.SignIn.Core.UseCases/SomeFile.cs"
                "README.md"
            )

            Mock dotnet -ParameterFilter { $args[0] -ceq "list" -and $args[1] -ceq "./src/Dfe.SignIn.PublicApi/Dfe.SignIn.PublicApi.csproj" } {
                return @(
                    "../Dfe.SignIn.Core.UseCases/Dfe.SignIn.Core.UseCases.csproj"
                )
            }
            Mock dotnet -ParameterFilter { $args[0] -ceq "list" -and $args[1] -ceq "./src/Dfe.SignIn.Web.SelectOrganisation/Dfe.SignIn.Web.SelectOrganisation.csproj" } {
                return @(
                    "../Dfe.SignIn.Core.UseCases/Dfe.SignIn.Core.UseCases.csproj"
                )
            }
        }

        It "should set 'SourceProjects' to the list of directly and indirectly associated source projects" {
            $result = & $Cmdlet -Path "." -Files $changedFiles

            $result.SourceProjects | Should -Be @(
                "Dfe.SignIn.Core.UseCases"
                "Dfe.SignIn.PublicApi"
                "Dfe.SignIn.Web.SelectOrganisation"
            )
        }
    }

    Context "when source and test files have been changed" {
        BeforeEach {
            $changedFiles = @(
                "src/Dfe.SignIn.Base.Framework/SomeFile.cs"
                "src/Dfe.SignIn.Base.Framework/AnotherFile.cs"
                "tests/Dfe.SignIn.PublicApi.UnitTests/XyzTests.cs"
                "README.md"
            )
        }

        It "should set 'SourceProjects' to the list of associated source projects" {
            $result = & $Cmdlet -Path "." -Files $changedFiles

            $result.SourceProjects | Should -Be @(
                "Dfe.SignIn.Base.Framework"
            )
        }

        It "should set 'TestProjects' to the list of all associated test projects" {
            $result = & $Cmdlet -Path "." -Files $changedFiles

            $result.TestProjects | Should -Be @(
                "Dfe.SignIn.Base.Framework.UnitTests"
                "Dfe.SignIn.PublicApi.UnitTests"
                "Dfe.SignIn.TestHelpers"
            )
        }
    }

    Context "when an associated test project does not exist" {
        BeforeEach {
            $changedFiles = @(
                "src/Dfe.SignIn.Base.Framework/SomeFile.cs"
                "src/Dfe.SignIn.Base.Framework/AnotherFile.cs"
                "tests/Dfe.SignIn.PublicApi.UnitTests/XyzTests.cs"
                "README.md"
            )

            Mock Test-Path -ParameterFilter { $Path -ceq "./tests/Dfe.SignIn.Base.Framework.UnitTests/Dfe.SignIn.Base.Framework.UnitTests.csproj" } {
                return $false
            }
        }

        It "should set 'SourceProjects' to the list of associated source projects" {
            $result = & $Cmdlet -Path "." -Files $changedFiles

            $result.SourceProjects | Should -Be @(
                "Dfe.SignIn.Base.Framework"
            )
        }

        It "should set 'TestProjects' to the list of all associated test projects that actually exist" {
            $result = & $Cmdlet -Path "." -Files $changedFiles

            $result.TestProjects | Should -Be @(
                "Dfe.SignIn.PublicApi.UnitTests"
                "Dfe.SignIn.TestHelpers"
            )
        }
    }

    Context "when test helper files have been changed" {
        BeforeEach {
            $changedFiles = @(
                "tests/Dfe.SignIn.TestHelpers/TestHelper.cs"
            )
        }

        It "should set 'SourceProjects' to be empty" {
            $result = & $Cmdlet -Path "." -Files $changedFiles

            $result.SourceProjects | Should -Be @()
        }

        It "should set 'TestProjects' to the list of all associated test projects" {
            $result = & $Cmdlet -Path "." -Files $changedFiles

            $result.TestProjects | Should -Be @(
                "Dfe.SignIn.TestHelpers"
                "Dfe.SignIn.TestHelpers.UnitTests"
            )
        }
    }

    Context "when source projects are deployable" {
        BeforeEach {
            $changedFiles = @(
                "src/Dfe.SignIn.Base.Framework/AnotherFile.cs"
                "src/Dfe.SignIn.Base.Framework/SomeFile.cs"
                "src/Dfe.SignIn.PublicApi/AnotherFile.cs"
                "src/Dfe.SignIn.Web.SelectOrganisation/AnotherFile.cs"
                "tests/Dfe.SignIn.InternalApi.UnitTests/XyzTests.cs"
                "README.md"
            )
        }

        It "should set 'DeployableProjects' to the list of deployable source projects" {
            $result = & $Cmdlet -Path "." -Files $changedFiles

            $result.DeployableProjects | Should -Be @(
                "Dfe.SignIn.PublicApi"
                "Dfe.SignIn.Web.SelectOrganisation"
            )
        }
    }
}
