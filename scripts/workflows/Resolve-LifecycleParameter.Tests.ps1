BeforeAll {
    $Cmdlet = $PSCommandPath.Replace('.Tests.ps1','.ps1')
}

Describe "Resolve-LifecycleParameter" {
    Context "when an override is not specified" {
        It "should assume the implied lifecycle 'dev'" {
            $result = & $Cmdlet -Implied 'dev'

            $result | Should -Be 'dev'
        }

        It "should assume the implied lifecycle 'rel'" {
            $result = & $Cmdlet -Implied 'rel'

            $result | Should -Be 'rel'
        }
    }

    Context "when an short form override is specified" {
        It "should assume override -> 'dev' -> 'rel'" {
            $result = & $Cmdlet -Implied 'dev' -Override 'rel'

            $result | Should -Be 'rel'
        }

        It "should assume override -> 'dev' -> 'dev'" {
            $result = & $Cmdlet -Implied 'dev' -Override 'dev'

            $result | Should -Be 'dev'
        }
    }

    Context "when an full form override is specified" {
        It "should assume override -> 'rel' -> 'Development'" {
            $result = & $Cmdlet -Implied 'rel' -Override 'Development'

            $result | Should -Be 'dev'
        }

        It "should assume override -> 'rel' -> 'development'" {
            $result = & $Cmdlet -Implied 'rel' -Override 'development'

            $result | Should -Be 'dev'
        }
    }

    Context "when an invalid lifecycle is specified" {
        It "throws when invalid lifecycle override is given" {
            {
                & $Cmdlet -Implied 'dev' -Override 'def'
            } | Should -Throw -ExpectedMessage "Invalid lifecycle 'def'."
        }
    }
}
