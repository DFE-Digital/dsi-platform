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
            $deserialized = $Value -replace 'dotnet_images=', '' | ConvertFrom-Json
            $global:capturedValue = $deserialized | ForEach-Object {
                @{ Project = $_.Project; Repository = $_.Repository }
            }
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

            $global:capturedValue.Project | Should -Be 'Dfe.SignIn.PublicApi'
            $global:capturedValue.Repository | Should -Be 'dev/public-api'
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

            $global:capturedValue.Project | Should -Be 'Dfe.SignIn.PublicApi'
            $global:capturedValue.Repository | Should -Be 'dev/public-api'
        }

        It "uses container repository for release lifecycle" {
            & $Cmdlet `
                -LifecycleName 'rel' `
                -ForceBuildPublicApi $true

            Should -Invoke Add-Content -Times 1 -Exactly

            $global:capturedValue.Project | Should -Be 'Dfe.SignIn.PublicApi'
            $global:capturedValue.Repository | Should -Be 'rel/public-api'
        }
    }

    Context "a specific project has been specified with an input" {
        It "includes project 'Dfe.SignIn.PublicApi'" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -ForceBuildPublicApi $true

            Should -Invoke Add-Content -Times 1 -Exactly

            $global:capturedValue.Project | Should -Be 'Dfe.SignIn.PublicApi'
            $global:capturedValue.Repository | Should -Be 'dev/public-api'
        }

        It "includes project 'Dfe.SignIn.Web.SelectOrganisation'" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -ForceBuildSelectOrganisation $true

            Should -Invoke Add-Content -Times 1 -Exactly

            $global:capturedValue.Project | Should -Be 'Dfe.SignIn.Web.SelectOrganisation'
            $global:capturedValue.Repository | Should -Be 'dev/select-organisation'
        }
    }
}
