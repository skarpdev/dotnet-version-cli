# Taken from psake https://github.com/psake/psake

<#  
.SYNOPSIS
  This is a helper function that runs a scriptblock and checks the PS variable $lastexitcode
  to see if an error occcured. If an error is detected then an exception is thrown.
  This function allows you to run command-line programs without having to
  explicitly check the $lastexitcode variable.
.EXAMPLE
  exec { svn info $repository_trunk } "Error executing SVN. Please verify SVN command-line client is installed"
#>
function Exec  
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd)
    )
    & $cmd
    if ($lastexitcode -ne 0) {
        throw ("Exec: " + $errorMessage)
    }
}

if(Test-Path .\artifacts) { Remove-Item .\artifacts -Force -Recurse }

$revision = @{ $true = $env:APPVEYOR_BUILD_NUMBER; $false = 1 }[$env:APPVEYOR_BUILD_NUMBER -ne $NULL];
$revision = "{0:D4}" -f [convert]::ToInt32($revision, 10)

#
# install Sonar Scanner (from SonarQube)
#
exec { & dotnet tool install --global dotnet-sonarscanner }

exec { & dotnet restore }

$sonarProjectKey = "skarpdev_dotnet-version-cli"
$sonarHostUrl = "https://sonarcloud.io"
$openCoveragePaths = "$Env:APPVEYOR_BUILD_FOLDER/test/coverage.*.opencover.xml"
$trxCoveragePahts = "$Env:APPVEYOR_BUILD_FOLDER/test/TestResults/*.trx"

# initialize Sonar Scanner
# If the environment variable APPVEYOR_PULL_REQUEST_NUMBER is not present, then this is not a pull request
if(-not $env:APPVEYOR_PULL_REQUEST_NUMBER) {
    exec {
        & dotnet sonarscanner begin `
            /k:$sonarProjectKey `
            /o:skarp `
            /v:$revision `
            /d:sonar.host.url=$sonarHostUrl `
            /d:sonar.cs.opencover.reportsPaths=$openCoveragePaths `
            /d:sonar.cs.vstest.reportsPaths=$trxCoveragePahts `
            /d:sonar.coverage.exclusions="**Test*.cs" `
            /d:sonar.login="$Env:SONARCLOUD_TOKEN"
    }
} else {
    exec {
        & dotnet sonarscanner begin `
            /k:$sonarProjectKey `
            /o:skarp `
            /v:$revision `
            /d:sonar.host.url=$sonarHostUrl `
            /d:sonar.cs.opencover.reportsPaths=$openCoveragePaths `
            /d:sonar.cs.vstest.reportsPaths=$trxCoveragePahts `
            /d:sonar.coverage.exclusions="**Test*.cs" `
            /d:sonar.login="$Env:SONARCLOUD_TOKEN" `
            /d:sonar.pullrequest.branch=$Env:APPVEYOR_PULL_REQUEST_HEAD_REPO_BRANCH `
            /d:sonar.pullrequest.base=$Env:APPVEYOR_REPO_BRANCH `
            /d:sonar.pullrequest.key=$Env:APPVEYOR_PULL_REQUEST_NUMBER
    }
}

exec { & dotnet build -c Release }

exec { & dotnet test .\test\dotnet-version-test.csproj -c Release /p:CollectCoverage=true /p:CoverletOutputFormat=opencover --logger trx }

# trigger Sonar Scanner analysis
exec { & dotnet sonarscanner end /d:sonar.login="$Env:SONARCLOUD_TOKEN" }

# pack up everything
exec { & dotnet pack .\src\dotnet-version.csproj -c Release -o ..\artifacts --include-source }