BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1', '.ps1')
}

Describe "Promote-NuGetPackage" {
    $testCases = @(
        @{
            PackagePath    = "/home/runner/work/dsi-platform/dsi-platform/src/Dfe.SignIn.PublicApi.Client.AspNetCore/bin/Release/Dfe.SignIn.PublicApi.Client.AspNetCore.1.0.9326.23534.nupkg"
            LifecycleStage = "dev"
            Pat            = "fake-personal-access-token"
            BaseFeedUrl    = "https://feeds.dev.azure.com/myOrg/myProj/_apis/packaging/feeds/myFeed/nuget/packages/"
            ExpectedUrl    = "https://feeds.dev.azure.com/myOrg/myProj/_apis/packaging/feeds/myFeed/nuget/packages/Dfe.SignIn.PublicApi.Client.AspNetCore/versions/1.0.9326.23534/views/Prerelease?api-version=7.1-preview.1"
        },
        @{
            PackagePath    = "/home/runner/work/dsi-platform/dsi-platform/src/Dfe.SignIn.PublicApi.Client/bin/Release/Dfe.SignIn.PublicApi.Client.1.4.9326.23537.nupkg"
            LifecycleStage = "rel"
            Pat            = "fake-personal-access-token"
            BaseFeedUrl    = "https://feeds.dev.azure.com/myOrg/myProj/_apis/packaging/feeds/myFeed/nuget/packages"
            ExpectedUrl    = "https://feeds.dev.azure.com/myOrg/myProj/_apis/packaging/feeds/myFeed/nuget/packages/Dfe.SignIn.PublicApi.Client/versions/1.4.9326.23537/views/Release?api-version=7.1-preview.1"
        }
    )

    Context "when called" {
        It "constructs the correct HTTP request" -TestCases $testCases {
            param (
                $PackagePath, $LifecycleStage, $Pat, $BaseFeedUrl, $ExpectedUrl
            )

            $global:capturedParams = $null

            Mock -CommandName Invoke-RestMethod -MockWith {
                $global:capturedParams = $PesterBoundParameters
            }

            & $Cmdlet `
                -PackagePath $PackagePath `
                -LifecycleStage $LifecycleStage `
                -Pat $Pat `
                -BaseFeedUrl $BaseFeedUrl

            Assert-MockCalled -CommandName Invoke-RestMethod -Times 1

            $captured = $global:capturedParams


            $captured['Method'] | Should -Be 'Put'
            $captured.Headers["Content-Type"]  | Should -Be "application/json"
            $captured.Headers["Authorization"] | Should -Be "Basic $([Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$pat")))"
            $captured['Uri'] | Should -Be $ExpectedUrl
            $captured['Body'] | Should -BeExactly $null
        }
    }
}
