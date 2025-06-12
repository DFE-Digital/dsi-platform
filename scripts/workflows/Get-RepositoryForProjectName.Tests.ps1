BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1','.ps1')
}

Describe "Get-RepositoryForProjectName" {
    It "throws when invalid lifecycle given" {
        {
            & $Cmdlet -ProjectName 'Dfe.SignIn.PublicApi' -LifecycleName 'abc'
        } | Should -Throw -ExpectedMessage "Invalid lifecycle 'abc'."
    }

    It "returns `$null when repository mapping was not specified" {
        $result = & $Cmdlet -ProjectName 'InvalidProject' -LifecycleName 'dev'

        $result | Should -Be $null
    }

    Context "project '<ProjectName>'" -ForEach @(
        @{ ProjectName = 'Dfe.SignIn.Web.SelectOrganisation'; Expected = 'select-organisation' }
        @{ ProjectName = 'Dfe.SignIn.PublicApi'; Expected = 'public-api-v2' }
    ) {
        It "should return the expected repository name for the 'dev' lifecycle" {
            $result = & $Cmdlet -ProjectName $ProjectName -LifecycleName 'dev'

            $result | Should -Be "$Expected"
        }

        It "should return the expected repository name for the 'rel' lifecycle" {
            $result = & $Cmdlet -ProjectName $ProjectName -LifecycleName 'rel'

            $result | Should -Be "$Expected-rel"
        }
    }
}
