BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1','.ps1')
}

Describe "Remove-UnnecessaryOutputFiles" {
    BeforeAll {
        Mock Resolve-Path {
            return "./docs/_site"
        }

        Mock Remove-Item {
            # Do nothing.
        }
    }

    It "should remove unused files" {
        & $Cmdlet -OutputPath "./docs/_site"

        Should -Invoke Remove-Item -ParameterFilter {
            $Path -contains "./docs/_site/manifest.json" -and `
            $Path -contains "./docs/_site/xrefmap.yml"
        } -Times 1 -Exactly
    }

    It "should remove intermediate search index" {
        & $Cmdlet -OutputPath "./docs/_site"

        Should -Invoke Remove-Item -ParameterFilter {
            $Path -contains "./docs/_site/index.json"
        } -Times 1 -Exactly
    }

    It "should remove intermediate toc.json files" {
        & $Cmdlet -OutputPath "./docs/_site"

        Should -Invoke Remove-Item -ParameterFilter {
            $Path -contains "./docs/_site/**/toc.json"
        } -Times 1 -Exactly
    }
}
