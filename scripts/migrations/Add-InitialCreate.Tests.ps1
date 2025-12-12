BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1', '.ps1')
}

Describe "New-InitialCreate" {

    BeforeAll {
        function Invoke-TestMigration {
            param (
                [Parameter(Mandatory)]
                [ValidateSet("directories", "organisations")]
                [string]$Context
            )
            & $Cmdlet -Context $Context
        }

        Mock dotnet {}
        Mock Write-Host {}
    }

    Context "When scaffolding the initial migration" {
        It "calls dotnet ef migrations add with correct parameters" {
            Invoke-TestMigration -Context "directories"

            Should -Invoke dotnet -Times 1 -ParameterFilter {
                $args[0] -ceq "ef" -and
                $args[1] -ceq "migrations" -and
                $args[2] -ceq "add" -and
                $args[3] -ceq "InitialCreate" -and
                $args[4] -ceq "--project" -and
                $args[5] -ceq "./migrations/migrations.csproj" -and
                $args[6] -ceq "--output-dir" -and
                $args[7] -ceq "Directories" -and
                $args[8] -ceq "--context" -and
                $args[9] -ceq "DbDirectoriesContext"
            }
        }
    }

    Context "When scaffolding organisations context" {
        It "constructs the correct context name and output directory" {
            Invoke-TestMigration -Context "organisations"

            Should -Invoke dotnet -Times 1 -ParameterFilter {
                $args[7] -ceq "Organisations" -and
                $args[9] -ceq "DbOrganisationsContext"
            }
        }
    }
}
