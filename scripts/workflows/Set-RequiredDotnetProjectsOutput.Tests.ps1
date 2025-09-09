BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1', '.ps1')
}

Describe "Set-RequiredDotnetProjectsOutput" {
    BeforeEach {
        $env:GITHUB_OUTPUT = 'GITHUB_OUTPUT'

        Mock Invoke-Command { @( 'Dfe.SignIn.PublicApi' ) }

        Mock Add-Content { }

        $global:capturedValue = $null
        Mock Add-Content -ParameterFilter { $Path -ceq 'GITHUB_OUTPUT' } {
            $global:capturedValue =$Value -replace 'dotnet_images=', '' | ConvertFrom-Json
        }
    }

    Context "no projects have been specified and none have been changed" {
        It "sets empty output array" {
            & $Cmdlet -LifecycleName 'dev'

            Should -Invoke Add-Content -Times 1 -Exactly

            $global:capturedValue | Should -Be @()
        }
    }

    Context "including projects that have been changed" {
        It "includes projects that have been changed" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -ChangedFiles @('src/Dfe.SignIn.PublicApi/Program.cs') `
                -IncludeChangedProjects $true

            Should -Invoke Add-Content -Times 1 -Exactly

            $global:capturedValue.project | Should -Be 'Dfe.SignIn.PublicApi'
            $global:capturedValue.repository | Should -Be 'dev/public-api'
        }
    }

    Context "projects are specified with inputs" {
        It "excludes projects that have been changed" {
            & $Cmdlet -LifecycleName 'dev'

            Should -Invoke Add-Content -Times 1 -Exactly

            $global:capturedValue | Should -Be @()
        }

        It "sets output array for multiple projects" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -ForceBuildPublicApi $true `
                -ForceBuildSelectOrganisation $true

            Should -Invoke Add-Content -Times 1 -Exactly

            $global:capturedValue[0].Project | Should -Be 'Dfe.SignIn.PublicApi'
            $global:capturedValue[0].Repository | Should -Be 'dev/public-api'
            $global:capturedValue[1].Project | Should -Be 'Dfe.SignIn.Web.SelectOrganisation'
            $global:capturedValue[1].Repository | Should -Be 'dev/select-organisation'
        }

        It "uses container repository for development lifecycle" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -ForceBuildPublicApi $true

            Should -Invoke Add-Content -Times 1 -Exactly

            $global:capturedValue.project | Should -Be 'Dfe.SignIn.PublicApi'
            $global:capturedValue.repository | Should -Be 'dev/public-api'
        }

        It "uses container repository for release lifecycle" {
            & $Cmdlet `
                -LifecycleName 'rel' `
                -ForceBuildPublicApi $true

            Should -Invoke Add-Content -Times 1 -Exactly

            $global:capturedValue.project | Should -Be 'Dfe.SignIn.PublicApi'
            $global:capturedValue.repository | Should -Be 'rel/public-api'
        }
    }

    Context "a specific project has been specified with an input" {
        It "includes project 'Dfe.SignIn.Fn.AuthExtensions'" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -ForceBuildAuthExtensions $true

            Should -Invoke Add-Content -Times 1 -Exactly

            $global:capturedValue.project | Should -Be 'Dfe.SignIn.Fn.AuthExtensions'
            $global:capturedValue.dockerfile | Should -Be 'functions'
            $global:capturedValue.repository | Should -Be 'dev/auth-extensions'
        }

        It "includes project 'Dfe.SignIn.PublicApi'" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -ForceBuildPublicApi $true

            Should -Invoke Add-Content -Times 1 -Exactly

            $global:capturedValue.project | Should -Be 'Dfe.SignIn.PublicApi'
            $global:capturedValue.dockerfile | Should -Be 'dotnet'
            $global:capturedValue.repository | Should -Be 'dev/public-api'
        }

        It "includes project 'Dfe.SignIn.Web.SelectOrganisation'" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -ForceBuildSelectOrganisation $true

            Should -Invoke Add-Content -Times 1 -Exactly

            $global:capturedValue.project | Should -Be 'Dfe.SignIn.Web.SelectOrganisation'
            $global:capturedValue.dockerfile | Should -Be 'dotnet'
            $global:capturedValue.repository | Should -Be 'dev/select-organisation'
        }
    }
}
