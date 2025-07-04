BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1', '.ps1')
}

Describe "Initialize-AzurePipeline.ps1" {
    Context "when building the request header" {
        It "should generate a valid Basic Auth header" {
            $token = 'fake-token'
            $expected = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$token"))

            $authBytes  = [Text.Encoding]::ASCII.GetBytes(":$token")
            $base64Auth = [Convert]::ToBase64String($authBytes)
            $headers = @{
                "Authorization" = "Basic $base64Auth"
                "Content-Type"  = "application/json"
            }

            $headers["Authorization"] | Should -BeExactly "Basic $expected"
            $headers["Content-Type"] | Should -BeExactly "application/json"
        }
    }

    Context "when building the body" {
        It "should build a valid JSON payload" {
            $branch = "refs/heads/test-branch"
            $projectName = "MyFakeApp"
            $repoName = "repo/container"
            $tag = "build-001"
            $TransformationEnv = $true
            $shortName = "App"

            $body = @{
                resources = @{
                    repositories = @{
                        self = @{
                            refName = $branch
                        }
                    }
                }
                templateParameters = @{
                    projectName = $projectName
                    repositoryName = $repoName
                    tag = $tag
                    tran = $TransformationEnv
                    applicationShortName = $shortName
                }
            } | ConvertTo-Json -Depth 5

            $parsed = $body | ConvertFrom-Json

            $parsed.resources.repositories.self.refName        | Should -Be $branch
            $parsed.templateParameters.projectName             | Should -Be $projectName
            $parsed.templateParameters.repositoryName          | Should -Be $repoName
            $parsed.templateParameters.tag                     | Should -Be $tag
            $parsed.templateParameters.tran                    | Should -Be $TransformationEnv
            $parsed.templateParameters.applicationShortName    | Should -Be $shortName
        }
    }

    Context "when building the url" {
        It "should create the correct Azure DevOps pipeline URL" {
            $orgProjectUrl = "https://dev.azure.com/example/project"
            $pipelineId = 123
            $url = "$orgProjectUrl/_apis/pipelines/$pipelineId/runs?api-version=7.1-preview.1"

            $url | Should -Be "https://dev.azure.com/example/project/_apis/pipelines/123/runs?api-version=7.1-preview.1"
        }
    }
}
