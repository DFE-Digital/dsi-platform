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
            Branch             = 'feature/xyz'
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
                $Token, $ProjectUrl, $PipelineId, $Branch, $TemplateParameters
            )

            $global:capturedParams = $null

            Mock -CommandName Invoke-RestMethod -MockWith {
                $global:capturedParams = $PesterBoundParameters
            }

            & $Cmdlet `
                -Token $Token `
                -ProjectUrl $ProjectUrl `
                -PipelineId $PipelineId `
                -Branch $Branch `
                -TemplateParameters $TemplateParameters

            Assert-MockCalled -CommandName Invoke-RestMethod -Times 1

            $captured = $global:capturedParams

            $capturedBody = $captured['Body'] | ConvertFrom-Json -Depth 5

            $captured['Method'] | Should -Be 'Post'
            $captured.Headers["Content-Type"]  | Should -Be "application/json"
            $captured.Headers["Authorization"] | Should -Be "Basic $([Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$Token")))"
            $captured['Uri'] | Should -Be "$ProjectUrl/_apis/pipelines/$pipelineId/runs?api-version=7.1"

            if ($Branch) {
                $capturedBody.resources.repositories.self.refName | Should -BeExactly "refs/heads/$Branch"
            }
            else {
                $capturedBody.resources.repositories.self.refName | Should -BeExactly "refs/heads/main"
            }

            $capturedBody.templateParameters.projectName | Should -Be $TemplateParameters.projectName
            $capturedBody.templateParameters.repositoryName | Should -Be $TemplateParameters.repositoryName
            $capturedBody.templateParameters.tag | Should -Be $TemplateParameters.tag
            $capturedBody.templateParameters.tran | Should -Be $TemplateParameters.tran
        }
    }
}
