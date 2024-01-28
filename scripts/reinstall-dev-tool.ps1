$ErrorActionPreference = "Stop"

function Assert-ExitCode {
    if (-not $?) {
        throw 'Latest command failed'
    }
}

$RepositoryRoot = Split-Path $PSScriptRoot

Push-Location
try
{
    Set-Location $RepositoryRoot

    [xml]$XmlConfig = Get-Content 'Directory.Build.props'

    $XmlElement = Select-Xml '/Project/PropertyGroup/VersionPrefix' $XmlConfig |
        Select-Object -ExpandProperty Node

    $VersionPrefix = $XmlElement.InnerText

    dotnet tool uninstall -g Heartbeat
    dotnet clean --configuration Release
    $VersionSuffix = "rc.$(Get-Date -Format 'yyyy-MM-dd-HHmm')"
    dotnet publish
    Assert-ExitCode
    dotnet pack --version-suffix $VersionSuffix
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



