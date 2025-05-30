BeforeAll {
    . $PSCommandPath.Replace('.Tests.ps1','.ps1')
}

Describe "Get-ProjectNamesFromFiles" {
    BeforeEach {
        Mock Get-ChildItem -ParameterFilter { $args[0] -eq "./src" } {
            return @(
                @{ Name = "Dfe.SignIn.Core.Framework" }
                @{ Name = "Dfe.SignIn.Core.UseCases" }
                @{ Name = "Dfe.SignIn.PublicApi" }
                @{ Name = "Dfe.SignIn.PublicApi.Client" }
                @{ Name = "Dfe.SignIn.Web.SelectOrganisation" }
                @{ Name = "Dfe.SignIn.WebFramework" }
            )
        }
        Mock Get-ChildItem -ParameterFilter { $args[0] -eq "./tests" } {
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
        Mock Get-Content {
            return "<Project></Project>"
        }
        Mock dotnet -ParameterFilter { $args[0] -eq "list" } {
            return @()
        }
    }

    Context "when source files have been changed" {
        BeforeEach {
            $changedFiles = @(
                "src/Dfe.SignIn.Core.Framework/SomeFile.cs"
                "src/Dfe.SignIn.Core.Framework/AnotherFile.cs"
                "src/Dfe.SignIn.WebFramework/Helpers.cs"
                "README.md"
            )
        }

        It "should set 'SourceProjects' to the list of associated source projects" {
            $result = Get-ProjectNamesFromFiles -Path "." -Files $changedFiles
            $result.SourceProjects | Should -Be @(
                "Dfe.SignIn.Core.Framework"
                "Dfe.SignIn.WebFramework"
            )
        }

        It "should set 'TestProjects' to the list of all associated test projects" {
            $result = Get-ProjectNamesFromFiles -Path "." -Files $changedFiles
            $result.TestProjects | Should -Be @(
                "Dfe.SignIn.Core.Framework.UnitTests"
                "Dfe.SignIn.TestHelpers"
                "Dfe.SignIn.TestHelpers.UnitTests"
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

            Mock dotnet -ParameterFilter { $args[0] -eq "list" -and $args[1] -eq "./src/Dfe.SignIn.PublicApi/Dfe.SignIn.PublicApi.csproj"} {
                return @(
                    "../Dfe.SignIn.Core.UseCases/Dfe.SignIn.Core.UseCases.csproj"
                )
            }
            Mock dotnet -ParameterFilter { $args[0] -eq "list" -and $args[1] -eq "./src/Dfe.SignIn.Web.SelectOrganisation/Dfe.SignIn.Web.SelectOrganisation.csproj"} {
                return @(
                    "../Dfe.SignIn.Core.UseCases/Dfe.SignIn.Core.UseCases.csproj"
                )
            }
        }

        It "should set 'SourceProjects' to the list of directly and indirectly associated source projects" {
            $result = Get-ProjectNamesFromFiles -Path "." -Files $changedFiles
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
                "src/Dfe.SignIn.Core.Framework/SomeFile.cs"
                "src/Dfe.SignIn.Core.Framework/AnotherFile.cs"
                "tests/Dfe.SignIn.PublicApi.UnitTests/XyzTests.cs"
                "README.md"
            )
        }

        It "should set 'TestProjects' to the list of associated source projects" {
            $result = Get-ProjectNamesFromFiles -Path "." -Files $changedFiles
            $result.SourceProjects | Should -Be @(
                "Dfe.SignIn.Core.Framework"
            )
        }

        It "should set 'TestProjects' to the list of all associated test projects" {
            $result = Get-ProjectNamesFromFiles -Path "." -Files $changedFiles
            $result.TestProjects | Should -Be @(
                "Dfe.SignIn.Core.Framework.UnitTests"
                "Dfe.SignIn.PublicApi.UnitTests"
                "Dfe.SignIn.TestHelpers"
                "Dfe.SignIn.TestHelpers.UnitTests"
            )
        }
    }

    Context "when source projects are deployable" {
        BeforeEach {
            $changedFiles = @(
                "src/Dfe.SignIn.Core.Framework/AnotherFile.cs"
                "src/Dfe.SignIn.Core.Framework/SomeFile.cs"
                "src/Dfe.SignIn.PublicApi/AnotherFile.cs"
                "src/Dfe.SignIn.Web.SelectOrganisation/AnotherFile.cs"
                "tests/Dfe.SignIn.InternalApi.UnitTests/XyzTests.cs"
                "README.md"
            )
        }

        It "should set 'DeployableProjects' to the list of deployable source projects" {
            $result = Get-ProjectNamesFromFiles -Path "." -Files $changedFiles
            $result.DeployableProjects | Should -Be @(
                "Dfe.SignIn.PublicApi"
                "Dfe.SignIn.Web.SelectOrganisation"
            )
        }
    }

    Context "when source projects are publishable packages" {
        BeforeEach {
            $changedFiles = @(
                "src/Dfe.SignIn.Core.Framework/AnotherFile.cs"
                "src/Dfe.SignIn.Core.Framework/SomeFile.cs"
                "src/Dfe.SignIn.PublicApi/AnotherFile.cs"
                "src/Dfe.SignIn.PublicApi.Client/AnotherFile.cs"
                "tests/Dfe.SignIn.InternalApi.UnitTests/XyzTests.cs"
                "README.md"
            )

            Mock Get-Content -ParameterFilter { $Path -eq "./src/Dfe.SignIn.Core.Framework/Dfe.SignIn.Core.Framework.csproj" } {
                return "<Project><IsPackable>true</IsPackable></Project>"
            }
            Mock Get-Content -ParameterFilter { $Path -eq "./src/Dfe.SignIn.PublicApi.Client/Dfe.SignIn.PublicApi.Client.csproj" } {
                return "<Project><IsPackable>true</IsPackable></Project>"
            }
        }

        It "should set 'PublishPackages' to true" {
            $result = Get-ProjectNamesFromFiles -Path "." -Files $changedFiles
            $result.PublishPackages | Should -Be $true
        }
    }

    Context "when no source projects are publishable packages" {
        BeforeEach {
            $changedFiles = @(
                "src/Dfe.SignIn.PublicApi/AnotherFile.cs"
                "src/Dfe.SignIn.Web.SelectOrganisation/AnotherFile.cs"
                "README.md"
            )
        }

        It "should set 'PublishPackages' to false" {
            $result = Get-ProjectNamesFromFiles -Path "." -Files $changedFiles
            $result.PublishPackages | Should -Be $false
        }
    }
}
