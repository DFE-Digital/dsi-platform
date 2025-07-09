<#
.SYNOPSIS
    Push Slack Message to desired Slack Channel

.DESCRIPTION
    Send notification to Slack

.PARAMETER SlackUri
    Slack Webhook URL

.PARAMETER SlackMessageHeading
    Slack message heading

.PARAMETER slackMainMessage
    Main Slack message

.PARAMETER SlackMainMessage
    Optional sub-message shown as pretext
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string] $SlackUri,

    [Parameter(Mandatory = $true)]
    [string] $SlackMessageHeading,

    [Parameter(Mandatory = $true)]
    [string] $SlackMainMessage,

    [string] $SlackSubMessage = ""
)

$attachment = @{
    text    = $SlackMainMessage
    color   = "#142954"
}
if ($slackSubMessage) {
    $attachment.pretext = $SlackSubMessage
}

$payload = @{
    username     = $SlackMessageHeading
    attachments  = @($attachment)
} | ConvertTo-Json -Depth 4

Invoke-RestMethod -Uri $SlackUri -Method Post -Body $payload -ContentType 'application/json'
Write-Host "Slack notification sent successfully."
