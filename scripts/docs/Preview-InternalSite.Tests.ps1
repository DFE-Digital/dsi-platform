BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1','.ps1')
}

Describe "Preview-InternalSite" {
    BeforeAll {
        Mock Invoke-Expression { }

        Mock dotnet { }

        Mock Resolve-Path {
            return "./docs"
        }
    }

    It "should build custom template" {
        & $Cmdlet

        Should -Invoke Invoke-Expression -Times 1 -ParameterFilter {
            $Command -match "Build-CustomTemplate"
        }
    }

    It "should generate the expected documentation" {
        & $Cmdlet

        Should -Invoke dotnet -Times 1 -ParameterFilter {
            $args[0] -eq "docfx" -and `
            $args[1] -eq "./docs/internal/docfx.json"
        }
    }

    It "should serve documentation on the expected port" {
        & $Cmdlet

        $expectedPort = 8086

        Should -Invoke dotnet -Times 1 -ParameterFilter {
            $args -contains "--serve" -and `
            $args -join " " -match "\s--port $expectedPort\b"
        }
    }
}
