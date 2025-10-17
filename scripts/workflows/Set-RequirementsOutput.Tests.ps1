BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1', '.ps1')
}

Describe "Set-RequirementsOutput" {
    BeforeEach {
        $env:GITHUB_OUTPUT = 'GITHUB_OUTPUT'

        Mock Invoke-Command { @( 'Dfe.SignIn.PublicApi' ) }

        Mock Add-Content { }

        $global:capturedValue = @{}
        Mock Add-Content -ParameterFilter { $Path -ceq 'GITHUB_OUTPUT' } {
            $parts = $Value -split '='
            $global:capturedValue[$parts[0]] = $parts[1]
        }
    }

    Context "flag check_powershell" {
        It "should be `$false by default" {
            $result = & $Cmdlet -LifecycleName 'dev'
            $global:capturedValue.check_powershell | Should -Be 'false'
        }

        It "should be `$true when forced" {
            $result = & $Cmdlet -LifecycleName 'dev' -ForceCheckPowershell 'true'
            $global:capturedValue.check_powershell | Should -Be 'true'
        }

        It "should be `$false when irrelevant files have been changed" {
            $result = & $Cmdlet -LifecycleName 'dev' -ChangedFiles @('README.md')
            $global:capturedValue.check_powershell | Should -Be 'false'
        }

        It "should be `$true when script files have been changed" {
            $result = & $Cmdlet -LifecycleName 'dev' -ChangedFiles @('scripts/Example.ps1')
            $global:capturedValue.check_powershell | Should -Be 'true'
        }
    }

    Context "flag check_dotnet" {
        It "should be `$false by default" {
            $result = & $Cmdlet -LifecycleName 'dev'
            $global:capturedValue.check_dotnet | Should -Be 'false'
        }

        It "should be `$true when forced" {
            $result = & $Cmdlet -LifecycleName 'dev' -ForceCheckDotnet 'true'
            $global:capturedValue.check_dotnet | Should -Be 'true'
        }

        It "should be `$false when irrelevant files have been changed" {
            $result = & $Cmdlet -LifecycleName 'dev' -ChangedFiles @('README.md')
            $global:capturedValue.check_dotnet | Should -Be 'false'
        }

        It "should be `$true when source files have been changed" {
            $result = & $Cmdlet -LifecycleName 'dev' -ChangedFiles @('src/Dfe.SignIn.PublicApi/Program.cs')
            $global:capturedValue.check_dotnet | Should -Be 'true'
        }

        It "should be `$true when test files have been changed" {
            $result = & $Cmdlet -LifecycleName 'dev' -ChangedFiles @('tests/Dfe.SignIn.PublicApi.UnitTests/ExampleTests.cs')
            $global:capturedValue.check_dotnet | Should -Be 'true'
        }
    }

    Context "flag check_docs_templates" {
        It "should be `$false by default" {
            $result = & $Cmdlet -LifecycleName 'dev'
            $global:capturedValue.check_docs_templates | Should -Be 'false'
        }

        It "should be `$true when forced" {
            $result = & $Cmdlet -LifecycleName 'dev' -ForceCheckDocs 'true'
            $global:capturedValue.check_docs_templates | Should -Be 'true'
        }

        It "should be `$false when irrelevant files have been changed" {
            $result = & $Cmdlet -LifecycleName 'dev' -ChangedFiles @('docs/namespaces/Dfe.SignIn.md')
            $global:capturedValue.check_docs_templates | Should -Be 'false'
        }

        It "should be `$true when documentation template files have been changed" {
            $result = & $Cmdlet -LifecycleName 'dev' -ChangedFiles @('docs/templates/package.json')
            $global:capturedValue.check_docs_templates | Should -Be 'true'
        }
    }

    Context "docker_images" {
        It "empty when no projects have been specified and none have been changed" {
            & $Cmdlet -LifecycleName 'dev'

            $dockerImages = $global:capturedValue.docker_images | ConvertFrom-Json
            $dockerImages | Should -Be @()
        }

        It "includes projects that have been changed" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -ChangedFiles @('src/Dfe.SignIn.PublicApi/Program.cs') `
                -BuildChangedProjects $true

            $dockerImages = $global:capturedValue.docker_images | ConvertFrom-Json
            $dockerImages | Should -HaveCount 1
            $dockerImages[0].project | Should -Be 'Dfe.SignIn.PublicApi'
            $dockerImages[0].dockerfile | Should -Be 'dotnet'
            $dockerImages[0].repository | Should -Be 'dev/public-api'
        }

        It "only includes projects that have been changed when '(n/a)' is provided for '-BuildForRepository'" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -ChangedFiles @('src/Dfe.SignIn.PublicApi/Program.cs') `
                -BuildChangedProjects $true `
                -BuildForRepository '(n/a)'

            $dockerImages = $global:capturedValue.docker_images | ConvertFrom-Json
            $dockerImages | Should -HaveCount 1
            $dockerImages[0].project | Should -Be 'Dfe.SignIn.PublicApi'
            $dockerImages[0].dockerfile | Should -Be 'dotnet'
            $dockerImages[0].repository | Should -Be 'dev/public-api'
        }

        It "includes all projects when '(all components)' is provided for '-BuildForRepository'" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -BuildForRepository '(all components)'

            $dockerImages = $global:capturedValue.docker_images | ConvertFrom-Json
            $dockerImages | Should -HaveCount 7
        }

        It "excludes projects that have been changed" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -ChangedFiles @('src/Dfe.SignIn.PublicApi/Program.cs') `
                -BuildChangedProjects $false

            $dockerImages = $global:capturedValue.docker_images | ConvertFrom-Json
            $dockerImages | Should -HaveCount 0
        }

        It "sets output array for multiple projects" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -ChangedFiles @('src/Dfe.SignIn.PublicApi/Program.cs') `
                -BuildChangedProjects $true `
                -BuildForRepository 'select-organisation'

            $dockerImages = $global:capturedValue.docker_images | ConvertFrom-Json
            $dockerImages | Should -HaveCount 2
            $dockerImages[0].project | Should -Be 'Dfe.SignIn.PublicApi'
            $dockerImages[0].dockerfile | Should -Be 'dotnet'
            $dockerImages[0].repository | Should -Be 'dev/public-api'
            $dockerImages[1].project | Should -Be 'Dfe.SignIn.Web.SelectOrganisation'
            $dockerImages[1].dockerfile | Should -Be 'dotnet'
            $dockerImages[1].repository | Should -Be 'dev/select-organisation'
        }

        It "throws when unexpected repository is specified" {
            {
                & $Cmdlet `
                    -LifecycleName 'dev' `
                    -BuildForRepository 'unexpected'
            } | Should -Throw -ExpectedMessage "Invalid repository name 'unexpected'."
        }

        It "uses container repository for development lifecycle" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -BuildForRepository 'public-api'

            $dockerImages = $global:capturedValue.docker_images | ConvertFrom-Json
            $dockerImages | Should -HaveCount 1
            $dockerImages[0].project | Should -Be 'Dfe.SignIn.PublicApi'
            $dockerImages[0].dockerfile | Should -Be 'dotnet'
            $dockerImages[0].repository | Should -Be 'dev/public-api'
        }

        It "uses container repository for release lifecycle" {
            & $Cmdlet `
                -LifecycleName 'rel' `
                -BuildForRepository 'public-api'

            $dockerImages = $global:capturedValue.docker_images | ConvertFrom-Json
            $dockerImages | Should -HaveCount 1
            $dockerImages[0].project | Should -Be 'Dfe.SignIn.PublicApi'
            $dockerImages[0].dockerfile | Should -Be 'dotnet'
            $dockerImages[0].repository | Should -Be 'rel/public-api'
        }

        It "includes 'frontend' project when force parameter provided" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -BuildForRepository 'frontend'

            $dockerImages = $global:capturedValue.docker_images | ConvertFrom-Json
            $dockerImages | Should -HaveCount 1
            $dockerImages[0].project | Should -Be 'frontend'
            $dockerImages[0].dockerfile | Should -Be 'frontend'
            $dockerImages[0].repository | Should -Be 'dev/frontend'
        }

        It "includes 'Dfe.SignIn.Fn.AuthExtensions' project when force parameter provided" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -BuildForRepository 'auth-extensions'

            $dockerImages = $global:capturedValue.docker_images | ConvertFrom-Json
            $dockerImages | Should -HaveCount 1
            $dockerImages[0].project | Should -Be 'Dfe.SignIn.Fn.AuthExtensions'
            $dockerImages[0].dockerfile | Should -Be 'functions'
            $dockerImages[0].repository | Should -Be 'dev/auth-extensions'
        }

        It "includes 'Dfe.SignIn.InternalApi' project when force parameter provided" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -BuildForRepository 'internal-api'

            $dockerImages = $global:capturedValue.docker_images | ConvertFrom-Json
            $dockerImages | Should -HaveCount 1
            $dockerImages[0].project | Should -Be 'Dfe.SignIn.InternalApi'
            $dockerImages[0].dockerfile | Should -Be 'dotnet'
            $dockerImages[0].repository | Should -Be 'dev/internal-api'
        }

        It "includes 'Dfe.SignIn.PublicApi' project when force parameter provided" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -BuildForRepository 'public-api'

            $dockerImages = $global:capturedValue.docker_images | ConvertFrom-Json
            $dockerImages | Should -HaveCount 1
            $dockerImages[0].project | Should -Be 'Dfe.SignIn.PublicApi'
            $dockerImages[0].dockerfile | Should -Be 'dotnet'
            $dockerImages[0].repository | Should -Be 'dev/public-api'
        }

        It "includes 'Dfe.SignIn.Web.Help' project when force parameter provided" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -BuildForRepository 'help'

            $dockerImages = $global:capturedValue.docker_images | ConvertFrom-Json
            $dockerImages | Should -HaveCount 1
            $dockerImages[0].project | Should -Be 'Dfe.SignIn.Web.Help'
            $dockerImages[0].dockerfile | Should -Be 'dotnet'
            $dockerImages[0].repository | Should -Be 'dev/help'
        }

        It "includes 'Dfe.SignIn.Web.SelectOrganisation' project when force parameter provided" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -BuildForRepository 'select-organisation'

            $dockerImages = $global:capturedValue.docker_images | ConvertFrom-Json
            $dockerImages | Should -HaveCount 1
            $dockerImages[0].project | Should -Be 'Dfe.SignIn.Web.SelectOrganisation'
            $dockerImages[0].dockerfile | Should -Be 'dotnet'
            $dockerImages[0].repository | Should -Be 'dev/select-organisation'
        }

        It "includes 'docs/external' project when force parameter provided" {
            & $Cmdlet `
                -LifecycleName 'dev' `
                -BuildForRepository 'developer'

            $dockerImages = $global:capturedValue.docker_images | ConvertFrom-Json
            $dockerImages | Should -HaveCount 1
            $dockerImages[0].project | Should -Be 'docs/external'
            $dockerImages[0].dockerfile | Should -Be 'docs'
            $dockerImages[0].repository | Should -Be 'dev/developer'
        }
    }

    Context "flag publish_dotnet_packages" {
        It "should be `$false by default" {
            $result = & $Cmdlet -LifecycleName 'dev'
            $global:capturedValue.publish_dotnet_packages | Should -Be 'false'
        }

        It "should be `$true when forced" {
            $result = & $Cmdlet -LifecycleName 'dev' -ForcePublishDotnetPackages 'true'
            $global:capturedValue.publish_dotnet_packages | Should -Be 'true'
        }

        It "should be `$false when irrelevant files have been changed" {
            $result = & $Cmdlet -LifecycleName 'dev' -ChangedFiles @('docs/internal/index.md')
            $global:capturedValue.publish_dotnet_packages | Should -Be 'false'
        }

        It "should be `$true when package <name> files have been changed" -ForEach @(
            @{ Name = 'Dfe.SignIn.Base.Framework' }
            @{ Name = 'Dfe.SignIn.Core.Public' }
            @{ Name = 'Dfe.SignIn.PublicApi.Client' }
            @{ Name = 'Dfe.SignIn.PublicApi.Client.AspNetCore' }
        ) {
            $result = & $Cmdlet -LifecycleName 'dev' -ChangedFiles @("src/$Name/Example.cs")
            $global:capturedValue.publish_dotnet_packages | Should -Be 'true'
        }
    }
}
