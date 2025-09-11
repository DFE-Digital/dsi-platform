BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1', '.ps1')
}

Describe "Start-Site" {
    BeforeAll {
        Mock Resolve-Path {
            return "."
        }

        Mock docker { }
    }

    Context "when previewing <Name> documentation" -ForEach @(
        @{
            Name                = 'external'
            ExpectedPort        = 8085
            ExpectedProjectName = 'docs/external'
            ExpectedTagName     = 'external-docs'
        }
        @{
            Name                = 'internal'
            ExpectedPort        = 8086
            ExpectedProjectName = 'docs/internal'
            ExpectedTagName     = 'internal-docs'
        }
        @{
            Name                = 'test'
            ExpectedPort        = 8087
            ExpectedProjectName = 'docs/templates/Dfe.SignIn.DocfxPlugin.Tests/sample-docs'
            ExpectedTagName     = 'test-docs'
        }
    ) {
        It "should build the correct dockerfile" {
            & $Cmdlet -Name $Name

            Should -Invoke docker -ParameterFilter {
                $args[0] -ceq "build" -and $args -join " " -match '-f \./docker/docs/Dockerfile'
            } -Times 1 -Exactly
        }

        It "should build with the expected project name" {
            & $Cmdlet -Name $Name

            Should -Invoke docker -ParameterFilter {
                $args[0] -ceq "build" -and $args -join " " -match "--build-arg PROJECT_NAME=$ExpectedProjectName"
            } -Times 1 -Exactly
        }

        It "should run on port <ExpectedPort>" {
            & $Cmdlet -Name $Name

            Should -Invoke docker -ParameterFilter {
                $args[0] -ceq "run" -and $args -join " " -match "-p $($ExpectedPort):8080"
            } -Times 1 -Exactly
        }

        It "should run with http://localhost:8081 for the CDN" {
            & $Cmdlet -Name $Name

            Should -Invoke docker -ParameterFilter {
                $args[0] -ceq "run" -and $args -join " " -match '-e CDN_BASE_ADDRESS=http://localhost:8081/'
            } -Times 1 -Exactly
        }

        It "should run with the expected image tag" {
            & $Cmdlet -Name $Name

            Should -Invoke docker -ParameterFilter {
                $args[0] -ceq "run" -and $args -join " " -match "$ExpectedTagName$"
            } -Times 1 -Exactly
        }
    }
}
