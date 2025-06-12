BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1', '.ps1')
}

Describe "Set-OutputFlags" {
    BeforeAll {
        Mock Add-Content { }

        $env:GITHUB_OUTPUT = 'GITHUB_OUTPUT.txt'
    }

    Context "when no change files and flags have not been specified" {
        It "should set no flags" {
            & $Cmdlet -ChangedFiles @() -Flags @{}

            Should -Invoke Add-Content -Times 0 -Exactly
        }
    }

    Context "when flags have not been specified" {
        It "should set no flags" {
            $changedFiles = @( 'src/example.txt' )

            & $Cmdlet -ChangedFiles $changedFiles -Flags @{}

            Should -Invoke Add-Content -Times 0 -Exactly
        }
    }

    Context "when one flag has been specified" {
        It "should not set flag when input is 'false'" {
            $changedFiles = @( 'src/example.txt' )

            & $Cmdlet -ChangedFiles $changedFiles -Flags @{
                flag1 = @{
                    Input = 'false'
                }
            }

            Should -Invoke Add-Content -ParameterFilter {
                $Path -ceq 'GITHUB_OUTPUT.txt' -and `
                    $Value -ceq 'flag1=false'
            } -Times 1 -Exactly
        }

        It "should set flag when input is 'true'" {
            $changedFiles = @( 'src/example.txt' )

            & $Cmdlet -ChangedFiles $changedFiles -Flags @{
                flag1 = @{
                    Input = 'true'
                }
            }

            Should -Invoke Add-Content -ParameterFilter {
                $Path -ceq 'GITHUB_OUTPUT.txt' -and `
                    $Value -ceq 'flag1=true'
            } -Times 1 -Exactly
        }

        It "should not set flag when a pattern specified but not matched" {
            $changedFiles = @( 'src/example.txt' )

            & $Cmdlet -ChangedFiles $changedFiles -Flags @{
                flag1 = @{
                    Patterns = "^abc/"
                }
            }

            Should -Invoke Add-Content -ParameterFilter {
                $Path -ceq 'GITHUB_OUTPUT.txt' -and `
                    $Value -ceq 'flag1=false'
            } -Times 1 -Exactly
        }

        It "should set flag when a single pattern specified and matched" {
            $changedFiles = @( 'src/example.txt' )

            & $Cmdlet -ChangedFiles $changedFiles -Flags @{
                flag1 = @{
                    Patterns = "^src/"
                }
            }

            Should -Invoke Add-Content -ParameterFilter {
                $Path -ceq 'GITHUB_OUTPUT.txt' -and `
                    $Value -ceq 'flag1=true'
            } -Times 1 -Exactly
        }

        It "should set flag when multiple patterns are specified and matched" {
            $changedFiles = @( 'src/example.txt' )

            & $Cmdlet -ChangedFiles $changedFiles -Flags @{
                flag1 = @{
                    Patterns = @(
                        "^abc/"
                        "^src/"
                    )
                }
            }

            Should -Invoke Add-Content -ParameterFilter {
                $Path -ceq 'GITHUB_OUTPUT.txt' -and `
                    $Value -ceq 'flag1=true'
            } -Times 1 -Exactly
        }
    }

    Context "when multiple flags have been specified" {
        It "should set multiple flags" {
            $changedFiles = @( 'src/example.txt' )

            & $Cmdlet -ChangedFiles $changedFiles -Flags @{
                flag1 = @{
                    Input = 'true'
                }
                flag2 = @{
                    Input    = 'false'
                    Patterns = "^src/"
                }
                flag3 = @{
                    Input = 'false'
                }
            }

            Should -Invoke Add-Content -ParameterFilter {
                $Path -ceq 'GITHUB_OUTPUT.txt' -and `
                    $Value -ceq 'flag1=true'
            } -Times 1 -Exactly
            Should -Invoke Add-Content -ParameterFilter {
                $Path -ceq 'GITHUB_OUTPUT.txt' -and `
                    $Value -ceq 'flag2=true'
            } -Times 1 -Exactly
            Should -Invoke Add-Content -ParameterFilter {
                $Path -ceq 'GITHUB_OUTPUT.txt' -and `
                    $Value -ceq 'flag3=false'
            } -Times 1 -Exactly
        }
    }
}
