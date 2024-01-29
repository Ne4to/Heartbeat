$ErrorActionPreference = "Stop"

function Assert-ExitCode {
    if (-not $?) {
        throw 'Latest command failed'
    }
}

$RepositoryRoot = Split-Path $PSScriptRoot
$SpaRoot = Join-Path $RepositoryRoot 'src/Heartbeat/ClientApp'
$PublishProject = Join-Path $RepositoryRoot 'src/Heartbeat/Heartbeat.csproj'

Push-Location
try
{
    $SpaRoot
    Set-Location $SpaRoot
    npm install
    npm run build

    Set-Location $RepositoryRoot

    [xml]$XmlConfig = Get-Content 'Directory.Build.props'

    $XmlElement = Select-Xml '/Project/PropertyGroup/VersionPrefix' $XmlConfig |
        Select-Object -ExpandProperty Node

    $VersionPrefix = $XmlElement.InnerText

    dotnet tool uninstall -g Heartbeat
    dotnet clean --configuration Release
    $VersionSuffix = "rc.$(Get-Date -Format 'yyyy-MM-dd-HHmm')"
    dotnet publish $PublishProject
    Assert-ExitCode
    dotnet pack --version-suffix $VersionSuffix $PublishProject
    Assert-ExitCode
    $PackageVersion = "$VersionPrefix-$VersionSuffix"
    dotnet tool install --global --add-source ./src/Heartbeat/nupkg Heartbeat --version $PackageVersion
    Assert-ExitCode
}
catch {
    Write-Host 'Install global tool - FAILED!' -ForegroundColor Red
    throw
}
finally {
    Pop-Location
}



