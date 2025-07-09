BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1', '.ps1')
}

Describe "Initialize-AzurePipeline" {

    $testCases = @(
        @{
            Pat               = '7456383-1'
            OrgProjectUrl     = 'https://dev.azure.com/fakeOrg1'
            PipelineId        = '101'
            BranchRef         = 'refs/heads/main'
            ProjectName       = 'ProjectOne'
            RepositoryName    = 'repo/one'
            Tag               = 'build-001'
            TransformationEnv = $true
        },
        @{
            Pat               = '762345873-2'
            OrgProjectUrl     = 'https://dev.azure.com/fakeOrg2'
            PipelineId        = '202'
            BranchRef         = 'refs/heads/develop'
            ProjectName       = 'ProjectTwo'
            RepositoryName    = 'repo/two'
            Tag               = 'build-999'
            TransformationEnv = $false
        }
    )

    Context "when called" {
        It "constructs the correct HTTP request" -TestCases $testCases {
            param (
                $Pat, $OrgProjectUrl, $PipelineId, $BranchRef,
                $ProjectName, $RepositoryName, $Tag, $TransformationEnv
            )

            $global:capturedParams = $null

            Mock -CommandName Invoke-RestMethod -MockWith {
                $global:capturedParams = $PesterBoundParameters
            }

            & $Cmdlet `
                -Pat $Pat `
                -OrgProjectUrl $OrgProjectUrl `
                -PipelineId $PipelineId `
                -BranchRef $BranchRef `
                -ProjectName $ProjectName `
                -RepositoryName $RepositoryName `
                -Tag $Tag `
                -TransformationEnv $TransformationEnv

            Assert-MockCalled -CommandName Invoke-RestMethod -Times 1

            $expectedBody = @{
                resources          = @{
                    repositories = @{
                        self = @{
                            refName = $BranchRef
                        }
                    }
                }
                templateParameters = @{
                    projectName    = $ProjectName
                    repositoryName = $RepositoryName
                    tag            = $Tag
                    tran           = $TransformationEnv
                }
            } | ConvertTo-Json -Depth 5

            $captured = $global:capturedParams

            $captured['Method'] | Should -Be 'Post'
            $captured.Headers["Content-Type"]  | Should -Be "application/json"
            $captured.Headers["Authorization"] | Should -Be "Basic $([Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$pat")))"
            $captured['Uri'] | Should -Be "$orgProjectUrl/_apis/pipelines/$pipelineId/runs?api-version=7.1"
            $captured['Body'] | Should -BeExactly $expectedBody
        }
    }
}
