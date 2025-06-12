BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1','.ps1')
}

Describe "Initialize-BuildSolution" {
    BeforeAll {
        Mock Resolve-Path {
            return "."
        }

        Mock Test-Path {
            return $true
        }

        Mock dotnet { }
    }

    Context "when no projects are given" {
        It "should create solution" {
            & $Cmdlet -Projects @{}

            Should -Invoke dotnet -Times 1 -ParameterFilter {
                $args[0] -ceq 'new' -and `
                $args[1] -ceq 'sln'
            }
        }

        It "should not attempt to add projects to solution" {
            & $Cmdlet -Projects @{}

            Should -Invoke dotnet -Times 0 -ParameterFilter {
                $args[0] -ceq 'sln' -and `
                $args[1] -ceq './build.sln' -and `
                $args[2] -ceq 'add'
            }
        }
    }

    Context "when source and test projects are given" {
        It "should create solution" {
            & $Cmdlet -Projects @{
                SourceProjects = @( 'Dfe.SignIn.PublicApi' )
                TestProjects = @( 'Dfe.SignIn.PublicApi.UnitTests' )
            }

            Should -Invoke dotnet -Times 1 -ParameterFilter {
                $args[0] -ceq 'new' -and `
                $args[1] -ceq 'sln'
            }
        }

        It "should add source project to solution" {
            & $Cmdlet -Projects @{
                SourceProjects = @( 'Dfe.SignIn.PublicApi' )
            }

            Should -Invoke dotnet -Times 1 -ParameterFilter {
                $args[0] -ceq 'sln' -and `
                $args[1] -ceq './build.sln' -and `
                $args[2] -ceq 'add' -and `
                $args[3] -ceq './src/Dfe.SignIn.PublicApi/Dfe.SignIn.PublicApi.csproj'
            }
        }

        It "should exclude source projects that do not exist" {
            Mock Test-Path -ParameterFilter { $Path -ceq './src/DoesNotExist/DoesNotExist.csproj' } {
                return $false
            }

            & $Cmdlet -Projects @{
                SourceProjects = @( 'DoesNotExist' )
            }

            Should -Invoke dotnet -ParameterFilter {
                $args[0] -ceq 'sln' -and `
                    $args[1] -ceq './build.sln' -and `
                    $args[2] -ceq 'add' -and `
                    $args[3] -ceq './src/DoesNotExist/DoesNotExist.csproj'
            } -Times 0 -Exactly
        }

        It "should add test project to solution" {
            & $Cmdlet -Projects @{
                TestProjects = @( 'Dfe.SignIn.PublicApi.UnitTests' )
            }

            Should -Invoke dotnet -Times 1 -ParameterFilter {
                $args[0] -ceq 'sln' -and `
                $args[1] -ceq './build.sln' -and `
                $args[2] -ceq 'add' -and `
                $args[3] -ceq './tests/Dfe.SignIn.PublicApi.UnitTests/Dfe.SignIn.PublicApi.UnitTests.csproj'
            }
        }

        It "should exclude test projects that do not exist" {
            Mock Test-Path -ParameterFilter { $Path -ceq './tests/DoesNotExist.UnitTests/DoesNotExist.UnitTests.csproj' } {
                return $false
            }

            & $Cmdlet -Projects @{
                TestProjects = @( 'DoesNotExist.UnitTests' )
            }

            Should -Invoke dotnet -ParameterFilter {
                $args[0] -ceq 'sln' -and `
                    $args[1] -ceq './build.sln' -and `
                    $args[2] -ceq 'add' -and `
                    $args[3] -ceq './tests/DoesNotExist.UnitTests/DoesNotExist.UnitTests.csproj'
            } -Times 0 -Exactly
        }
    }
}
