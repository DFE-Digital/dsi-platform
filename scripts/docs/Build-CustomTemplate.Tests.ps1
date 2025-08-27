BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1', '.ps1')
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

        Should -Invoke pwsh -ParameterFilter {
            $args -join " " -match "-WorkingDirectory \./docs/templates" -and `
                $args -join " " -match "npm install" -and `
                $args -join " " -match "npm run build"
        } -Times 1 -Exactly
    }

    It "should build docfx post-processing plugin" {
        & $Cmdlet

        Should -Invoke dotnet -ParameterFilter {
            $args[0] -ceq "build" -and `
                $args[1] -ceq "./docs/templates"
        } -Times 1 -Exactly
    }
}
