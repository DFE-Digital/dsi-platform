BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1', '.ps1')
}

Describe "New-Migration" {
    BeforeAll {
        function Invoke-CmdLet {
            & $Cmdlet `
                -Context "directories" `
                -MigrationName "MockMigration"
        }

        Mock dotnet {}
        Mock Write-Host {}
    }

    Context "When creating a new migration" {
        It "should pass the correct parameters to the dotnet ef command" {
            Invoke-CmdLet

            Should -Invoke dotnet -ParameterFilter {
                $args[0] -ceq "ef" -and `
                    $args[1] -ceq "migrations" -and `
                    $args[2] -ceq "add" -and `
                    $args[3] -ceq "MockMigration" -and `
                    $args[4] -ceq "--project" -and `
                    $args[5] -ceq "./migrations/migrations.csproj" -and `
                    $args[6] -ceq "--output-dir" -and `
                    $args[7] -ceq "Directories" -and `
                    $args[8] -ceq "--context" -and `
                    $args[9] -ceq "DbDirectoriesContext"
            } -Times 1 -Exactly
        }
    }
}
