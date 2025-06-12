<#
.SYNOPSIS
    Sets output flags for the current job step.

.PARAMETER ChangedFiles
    The array of changed files.

.PARAMETER Flags
    An object where each key specifies a flag that is to be set.

    The following options can be specified for each flag:
        - Input - String. When 'true' indicates that flag should be set.
        - Patterns - String[]. An array of file patterns.

.OUTPUTS
    Writes flags to GitHub output and outputs a summary to the host.

.EXAMPLE
    ./scripts/workflows/Set-OutputFlags -ChangedFiles $changedFiles -Flags @{
      flag1 = @{
        Input = '${{ github.events.inputs.input_override1 }}'
        Patterns = "^src/"
      }
      flag2 = @{
        Input = '${{ github.events.inputs.input_override2 }}'
        Patterns = @(
            "^src/Project1/"
            "^src/Project2/"
        )
      }
    }
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [AllowEmptyCollection()]
    [String[]]$ChangedFiles,

    [Parameter(Mandatory = $true)]
    [PSCustomObject]$Flags
)

$ErrorActionPreference = "Stop"

foreach ($flag in $Flags.GetEnumerator()) {
    $value = $flag.Value.Input -eq 'true'

    if ($flag.Value.Patterns) {
        $pattern = $flag.Value.Patterns -join '|'
        $value = $value -or $ChangedFiles.Where({ $_ -match $pattern }, 'First').Count -gt 0
    }

    $value = "$value".ToLower()

    Write-Host "Set flag '$($flag.Key)=$value'"

    Add-Content -Path $env:GITHUB_OUTPUT -Value "$($flag.Key)=$value"
}
