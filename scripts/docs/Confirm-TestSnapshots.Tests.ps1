BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1','.ps1')
}

Describe "Confirm-TestSnapshots" {
    BeforeAll {
        Mock Resolve-Path {
            return "."
        }

        Mock Get-ChildItem -ParameterFilter { $Path -ceq "./templates/Dfe.SignIn.DocfxPlugin.Tests/snapshots/*.received.txt" } {
            return @(
                @{
                    Name = "snapshot_page1.html.verified.txt"
                    DirectoryName = "./templates/Dfe.SignIn.DocfxPlugin.Tests/snapshots"
                }
                @{
                    Name = "snapshot_page1.html.received.txt"
                    DirectoryName = "./templates/Dfe.SignIn.DocfxPlugin.Tests/snapshots"
                }
                @{
                    Name = "snapshot_page2.html.verified.txt"
                    DirectoryName = "./templates/Dfe.SignIn.DocfxPlugin.Tests/snapshots"
                }
                @{
                    Name = "snapshot_page3.html.received.txt"
                    DirectoryName = "./templates/Dfe.SignIn.DocfxPlugin.Tests/snapshots"
                }
            )
        }

        Mock Move-Item { }
    }

    It "should replace existing snapshot with new snapshot" {
        & $Cmdlet

        Should -Invoke Move-Item -Times 1 -ParameterFilter {
            $Path -ceq "./templates/Dfe.SignIn.DocfxPlugin.Tests/snapshots/snapshot_page1.html.received.txt" -and `
            $Destination -ceq "./templates/Dfe.SignIn.DocfxPlugin.Tests/snapshots/snapshot_page1.html.verified.txt" -and `
            $Force
        }
    }

    It "should do nothing where no new snapshot is present" {
        & $Cmdlet

        Should -Invoke Move-Item -Times 0 -ParameterFilter {
            $Path -ceq "./templates/Dfe.SignIn.DocfxPlugin.Tests/snapshots/snapshot_page2.html.received.txt" -and `
            $Destination -ceq "./templates/Dfe.SignIn.DocfxPlugin.Tests/snapshots/snapshot_page2.html.verified.txt"
        }
    }

    It "should accept new snapshot where no existing snapshot existed" {
        & $Cmdlet

        Should -Invoke Move-Item -Times 1 -ParameterFilter {
            $Path -ceq "./templates/Dfe.SignIn.DocfxPlugin.Tests/snapshots/snapshot_page3.html.received.txt" -and `
            $Destination -ceq "./templates/Dfe.SignIn.DocfxPlugin.Tests/snapshots/snapshot_page3.html.verified.txt"
        }
    }
}
