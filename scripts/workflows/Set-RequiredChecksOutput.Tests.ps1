BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1', '.ps1')
}

Describe "Set-RequiredChecksOutput" {
    BeforeEach {
        $env:GITHUB_OUTPUT = 'GITHUB_OUTPUT'

        Mock Add-Content { }

        $global:capturedValue = @{}
        Mock Add-Content -ParameterFilter { $Path -ceq 'GITHUB_OUTPUT' } {
            $parts = $Value -split '='
            $global:capturedValue[$parts[0]] = $parts[1] -eq 'true'
        }
    }

    Context "flag check_powershell" {
        It "should be `$false by default" {
            $result = & $Cmdlet
            $global:capturedValue.check_powershell | Should -Be $false
        }

        It "should be `$true when forced" {
            $result = & $Cmdlet -ForceCheckPowershell 'true'
            $global:capturedValue.check_powershell | Should -Be $true
        }

        It "should be `$false when irrelevant files have been changed" {
            $result = & $Cmdlet -ChangedFiles @('README.md')
            $global:capturedValue.check_powershell | Should -Be $false
        }

        It "should be `$true when script files have been changed" {
            $result = & $Cmdlet -ChangedFiles @('scripts/Example.ps1')
            $global:capturedValue.check_powershell | Should -Be $true
        }
    }

    Context "flag check_dotnet" {
        It "should be `$false by default" {
            $result = & $Cmdlet
            $global:capturedValue.check_dotnet | Should -Be $false
        }

        It "should be `$true when forced" {
            $result = & $Cmdlet -ForceCheckDotnet 'true'
            $global:capturedValue.check_dotnet | Should -Be $true
        }

        It "should be `$false when irrelevant files have been changed" {
            $result = & $Cmdlet -ChangedFiles @('README.md')
            $global:capturedValue.check_dotnet | Should -Be $false
        }

        It "should be `$true when source files have been changed" {
            $result = & $Cmdlet -ChangedFiles @('src/Dfe.SignIn.PublicApi/Program.cs')
            $global:capturedValue.check_dotnet | Should -Be $true
        }

        It "should be `$true when test files have been changed" {
            $result = & $Cmdlet -ChangedFiles @('tests/Dfe.SignIn.PublicApi.UnitTests/ExampleTests.cs')
            $global:capturedValue.check_dotnet | Should -Be $true
        }
    }

    Context "flag check_docs_templates" {
        It "should be `$false by default" {
            $result = & $Cmdlet
            $global:capturedValue.check_docs_templates | Should -Be $false
        }

        It "should be `$true when forced" {
            $result = & $Cmdlet -ForceCheckDocs 'true'
            $global:capturedValue.check_docs_templates | Should -Be $true
        }

        It "should be `$false when irrelevant files have been changed" {
            $result = & $Cmdlet -ChangedFiles @('docs/namespaces/Dfe.SignIn.md')
            $global:capturedValue.check_docs_templates | Should -Be $false
        }

        It "should be `$true when documentation template files have been changed" {
            $result = & $Cmdlet -ChangedFiles @('docs/templates/package.json')
            $global:capturedValue.check_docs_templates | Should -Be $true
        }
    }

    Context "flag build_external_docs" {
        It "should be `$false by default" {
            $result = & $Cmdlet
            $global:capturedValue.build_external_docs | Should -Be $false
        }

        It "should be `$true when forced" {
            $result = & $Cmdlet -ForceBuildExternalDocs 'true'
            $global:capturedValue.build_external_docs | Should -Be $true
        }

        It "should be `$false when irrelevant files have been changed" {
            $result = & $Cmdlet -ChangedFiles @('docs/internal/index.md')
            $global:capturedValue.build_external_docs | Should -Be $false
        }

        It "should be `$true when external documentation files have been changed" {
            $result = & $Cmdlet -ChangedFiles @('docs/external/index.md')
            $global:capturedValue.build_external_docs | Should -Be $true
        }

        It "should be `$false when namespace files have been changed" {
            $result = & $Cmdlet -ChangedFiles @('docs/namespaces/Dfe.SignIn.md')
            $global:capturedValue.build_external_docs | Should -Be $true
        }

        It "should be `$true when documentation template files have been changed" {
            $result = & $Cmdlet -ChangedFiles @('docs/templates/package.json')
            $global:capturedValue.build_external_docs | Should -Be $true
        }
    }

    Context "flag publish_dotnet_packages" {
        It "should be `$false by default" {
            $result = & $Cmdlet
            $global:capturedValue.publish_dotnet_packages | Should -Be $false
        }

        It "should be `$true when forced" {
            $result = & $Cmdlet -ForcePublishDotnetPackages 'true'
            $global:capturedValue.publish_dotnet_packages | Should -Be $true
        }

        It "should be `$false when irrelevant files have been changed" {
            $result = & $Cmdlet -ChangedFiles @('docs/internal/index.md')
            $global:capturedValue.publish_dotnet_packages | Should -Be $false
        }

        It "should be `$true when package <name> files have been changed" -ForEach @(
            @{ Name = 'Dfe.SignIn.Base.Framework' }
            @{ Name = 'Dfe.SignIn.Core.Public' }
            @{ Name = 'Dfe.SignIn.PublicApi.Client' }
            @{ Name = 'Dfe.SignIn.PublicApi.Client.AspNetCore' }
        ) {
            $result = & $Cmdlet -ChangedFiles @("src/$Name/Example.cs")
            $global:capturedValue.publish_dotnet_packages | Should -Be $true
        }
    }
}
