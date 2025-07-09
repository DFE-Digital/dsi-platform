@{
  Run          = @{
    Path = './scripts'
  }
  TestResult   = @{
    Enabled      = $true
    OutputFormat = 'JUnitXml'
    OutputPath   = './scripts/TestResults.xml'
  }
  CodeCoverage = @{
    Enabled      = $true
    Path         = './scripts'
    ExcludePath  = @(
      "./scripts/workflows/Get-ChangedFilesInBranch.ps1"
      "./scripts/workflows/Initialize-BuildSolutionForChangedProjects.ps1"
      "./scripts/workflows/Send-SlackNotification.ps1"
      "./scripts/workflows/Initialize-AzurePipeline.ps1"
    )
    OutputFormat = 'Cobertura'
    OutputPath   = './scripts/Coverage.xml'
  }
}
