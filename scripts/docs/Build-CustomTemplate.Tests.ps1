BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1','.ps1')
}

Describe "Build-CustomTemplate" {
    BeforeAll {
        Mock Resolve-Path {
            return "./docs"
        }

        Mock pwsh { }

        Mock dotnet { }
    }

    It "should install and build template scripts and styles" {
        & $Cmdlet

        Should -Invoke pwsh -Times 1 -ParameterFilter {
            $args -join " " -match "-WorkingDirectory \./docs/templates" -and `
            $args -join " " -match "npm install" -and `
            $args -join " " -match "npm run build"
        }
    }

    It "should build docfx post-processing plugin" {
        & $Cmdlet

        Should -Invoke dotnet -Times 1 -ParameterFilter {
            $args[0] -eq "build" -and `
            $args[1] -eq "./docs/templates"
        }
    }
}
