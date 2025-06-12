BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1', '.ps1')
}

Describe "Preview-TestSite" {
    BeforeAll {
        Mock Invoke-Expression { }

        Mock dotnet { }

        Mock Resolve-Path {
            return "./docs"
        }
    }

    It "should build custom template" {
        & $Cmdlet

        Should -Invoke Invoke-Expression -ParameterFilter {
            $Command -match "Build-CustomTemplate"
        } -Times 1 -Exactly
    }

    It "should generate the expected documentation" {
        & $Cmdlet

        Should -Invoke dotnet -ParameterFilter {
            $args[0] -ceq "docfx" -and `
                $args[1] -ceq "./docs/templates/Dfe.SignIn.DocfxPlugin.Tests/sample-docs/docfx.json"
        } -Times 1 -Exactly
    }

    It "should serve documentation on the expected port" {
        & $Cmdlet

        $expectedPort = 8087

        Should -Invoke dotnet -ParameterFilter {
            $args -contains "--serve" -and `
                $args -join " " -match "\s--port $expectedPort\b"
        } -Times 1 -Exactly
    }
}
