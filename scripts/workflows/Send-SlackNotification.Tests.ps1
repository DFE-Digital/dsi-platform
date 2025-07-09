BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1', '.ps1')
}

Describe "Send-SlackNotification" {
    $testCases = @(
        @{
            SlackUri            = "https://slack-url-fake-1/"
            SlackMessageHeading = "https://dev.azure.com/fakeOrg1"
            SlackMainMessage    = "101"
            SlackSubMessage     = "refs/heads/main"
            SlackIconEmoji      = ":rocket:"
        },
        @{
            SlackUri            = "https://slack-url-fake-2/"
            SlackMessageHeading = "https://dev.azure.com/fakeOrg1"
            SlackMainMessage    = "101"
            SlackSubMessage     = "refs/heads/main"
            SlackIconEmoji      = ":tada:"
        }
    )

    Context "when called" {
        It "constructs the correct HTTP request" -TestCases $testCases {
            param (
                $SlackUri, $SlackMessageHeading, $SlackMainMessage, $SlackSubMessage,
                $SlackIconEmoji
            )

            $global:capturedParams = $null

            Mock -CommandName Invoke-RestMethod -MockWith {
                $global:capturedParams = $PesterBoundParameters
            }

            & $Cmdlet `
                -SlackUri $SlackUri `
                -SlackMessageHeading $SlackMessageHeading `
                -SlackMainMessage $SlackMainMessage `
                -SlackSubMessage $SlackSubMessage `
                -SlackIconEmoji $SlackIconEmoji

            Assert-MockCalled -CommandName Invoke-RestMethod -Times 1

            $captured = $global:capturedParams

            $captured.Uri | Should -Be $SlackUri
            $captured.Method | Should -Be "Post"
            $captured.ContentType | Should -Be "application/json"

            $bodyObj = $captured.Body | ConvertFrom-Json
            $bodyObj.username | Should -Be $SlackMessageHeading
            $bodyObj.icon_emoji | Should -Be $SlackIconEmoji

            $expectedAttachment = @{
                text    = $SlackMainMessage
                color   = '#142954'
                pretext = $SlackSubMessage
            }

            $attachment = $bodyObj.attachments[0]
            $attachment.PSObject.Properties.value.Count | Should -Be 3
            foreach ($key in $expectedAttachment.Keys) {
                $attachment.$key | Should -BeExactly $expectedAttachment[$key]
            }
        }
    }
}
