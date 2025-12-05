BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1', '.ps1')
}

Describe "New-DbContext" {
    BeforeAll {

        function Invoke-TestDbContext {
            param(
                [string[]]$Tables = @()
            )

            & $Cmdlet `
                -ConnectionString "Server=.;Database=.;User ID=.;Password=.;TrustServerCertificate=True;" `
                -EntityPath "./src/Dfe.SignIn.Core.Entities/Directories" `
                -ProjectPath "./src/Dfe.SignIn.Gateways.EntityFramework" `
                -ModelOutputDir "../Dfe.SignIn.Core.Entities/Directories" `
                -ContextOutputDir "./" `
                -ModelNamespace "Dfe.SignIn.Core.Entities.Directories" `
                -ContextNamespace "Dfe.SignIn.Gateways.EntityFramework" `
                -ContextName "DbDirectoriesContext" `
                -Tables $Tables
        }

        Mock dotnet {}
        Mock Write-Host {}

        Mock Get-ChildItem {}
        Mock Rename-Item {}
        Mock Test-Path { $false }
    }

    Context "1. EF Scaffold Command" {
        It "builds and executes the correct dotnet ef command" {
            Invoke-TestDbContext -Tables @()

            Should -Invoke dotnet -ParameterFilter {
                $args[0] -ceq "ef" -and `
                    $args[1] -ceq "dbcontext" -and `
                    $args[2] -ceq "scaffold" -and `
                    $args[3] -ceq "Server=.;Database=.;User ID=.;Password=.;TrustServerCertificate=True;" -and `
                    $args[4] -ceq "Microsoft.EntityFrameworkCore.SqlServer" -and `
                    $args[5] -ceq "--project" -and `
                    $args[6] -ceq "./src/Dfe.SignIn.Gateways.EntityFramework" -and `
                    $args[7] -ceq "--output-dir" -and `
                    $args[8] -ceq "../Dfe.SignIn.Core.Entities/Directories" -and `
                    $args[9] -ceq "--context-dir" -and `
                    $args[10] -ceq "./" -and `
                    $args[11] -ceq "--namespace" -and `
                    $args[12] -ceq "Dfe.SignIn.Core.Entities.Directories" -and `
                    $args[13] -ceq "--context-namespace" -and `
                    $args[14] -ceq "Dfe.SignIn.Gateways.EntityFramework" -and `
                    $args[15] -ceq "--context" -and `
                    $args[16] -ceq "DbDirectoriesContext" -and `
                    $args[17] -ceq "--no-onconfiguring"
            } -Times 1 -Exactly
        }

        It "includes --table arguments for each table supplied" {
            Invoke-TestDbContext -Tables @("TableA", "TableB", "TableC")

            Should -Invoke dotnet -ParameterFilter {
                $args -contains "--table" -and `
                    $args -contains "TableA" -and `
                    $args -contains "--table" -and `
                    $args -contains "TableB" -and `
                    $args -contains "--table" -and `
                    $args -contains "TableC"
            } -Times 1 -Exactly
        }
    }

    Context "2. Rename Model Files to *Entity.cs*" {
        BeforeEach {
            $renamed = [System.Collections.Generic.List[string]]::new()

            $generatedFiles = @(
                [PSCustomObject]@{ Name = "User.cs"; FullName = "$EntityPath/User.cs" },
                [PSCustomObject]@{ Name = "Role.cs"; FullName = "$EntityPath/Role.cs" },
                [PSCustomObject]@{ Name = "UserEntity.cs"; FullName = "$EntityPath/UserEntity.cs" }
            )

            Mock Get-ChildItem {
                return $generatedFiles
            }

            Mock Rename-Item {
                param ($Path, $NewName)
                $renamed.Add("$Path -> $NewName")
            }
            Mock Write-Host {}
        }

        It "renames only files that do not already end with Entity.cs" {
            Invoke-TestDbContext -Tables @()

            $renamed | Should -Contain "$EntityPath/User.cs -> UserEntity.cs"
            $renamed | Should -Contain "$EntityPath/Role.cs -> RoleEntity.cs"
            $renamed | Should -Not -Contain "$EntityPath/UserEntity.cs -> UserEntityEntity.cs"
        }
    }

    Context "3. dotnet format" {
        It "is executed exactly once at the end" {
            Invoke-TestDbContext -Tables @()

            Should -Invoke dotnet -Times 1 -ParameterFilter {
                $args[0] -eq "format"
            }
        }
    }
}
