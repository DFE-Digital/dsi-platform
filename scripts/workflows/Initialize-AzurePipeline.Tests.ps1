BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1', '.ps1')
}

Describe "Initialize-AzurePipeline.ps1" {
    Context "when called with valid parameters" {
        It "returns the expected pipeline run URL" {

            Mock Invoke-RestMethod {
                @{ url = 'https://dev.azure.com/fakeOrg/_apis/pipelines/999/runs?api-version=7.1-preview.1' }
            }

            $pat = 'fake-pat-token'
            $orgProjectUrl = 'https://dev.azure.com/fakeOrg'
            $pipelineId = '999'
            $branchRef = 'refs/heads/main'
            $projectName = 'FakeProject'
            $repositoryName = 'fake/repo'
            $tag = 'build-001'
            $transformationEnv = $true
            $appShortName = 'FApp'

            $result = & $Cmdlet `
                -Pat $pat `
                -OrgProjectUrl $orgProjectUrl `
                -PipelineId $pipelineId `
                -BranchRef $branchRef `
                -ProjectName $projectName `
                -RepositoryName $repositoryName `
                -Tag $tag `
                -TransformationEnv $transformationEnv

            $result | Should -Be 'https://dev.azure.com/fakeOrg/_apis/pipelines/999/runs?api-version=7.1-preview.1'
        }
    }
}
