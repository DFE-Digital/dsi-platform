BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1', '.ps1')
}

Describe "Start-AzurePipeline" {

    $testCases = @(
        @{
            Token              = '7456383-1'
            ProjectUrl         = 'https://dev.azure.com/fakeOrg1'
            PipelineId         = '101'
            TemplateParameters = @{
                projectName    = 'ProjectOne'
                repositoryName = 'repo/one'
                tag            = 'build-001'
                tran           = $true
            }
        },
        @{
            Token              = '762345873-2'
            ProjectUrl         = 'https://dev.azure.com/fakeOrg2'
            PipelineId         = '202'
            TemplateParameters = @{
                projectName    = 'ProjectTwo'
                repositoryName = 'repo/two'
                tag            = 'build-999'
                tran           = $false
            }
        }
    )

    Context "when called" {
        It "constructs the correct HTTP request" -TestCases $testCases {
            param (
                $Token, $ProjectUrl, $PipelineId, $TemplateParameters
            )

            $global:capturedParams = $null

            Mock -CommandName Invoke-RestMethod -MockWith {
                $global:capturedParams = $PesterBoundParameters
            }

            & $Cmdlet `
                -Token $Token `
                -ProjectUrl $ProjectUrl `
                -PipelineId $PipelineId `
                -TemplateParameters $TemplateParameters

            Assert-MockCalled -CommandName Invoke-RestMethod -Times 1

            $expectedBody = @{
                templateParameters = $TemplateParameters
            } | ConvertTo-Json -Depth 5

            $captured = $global:capturedParams

            $captured['Method'] | Should -Be 'Post'
            $captured.Headers["Content-Type"]  | Should -Be "application/json"
            $captured.Headers["Authorization"] | Should -Be "Basic $([Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$Token")))"
            $captured['Uri'] | Should -Be "$ProjectUrl/_apis/pipelines/$pipelineId/runs?api-version=7.1"
            $captured['Body'] | Should -BeExactly $expectedBody
        }
    }
}
