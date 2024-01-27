$ErrorActionPreference = "Stop"

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
    Get-Date -Format ''
    $VersionSuffix = "rc.$(Get-Date -Format 'yyyy-MM-dd-HHmm')"
    dotnet publish --runtime win-x64
    dotnet pack --runtime win-x64 --version-suffix $VersionSuffix
#    $PackageVersion = "$VersionPrefix-$VersionSuffix"
#    dotnet tool install --global --add-source ./src/Heartbeat/nupkg Heartbeat --version $PackageVersion
}
catch {
    Write-Host 'Install global tool - FAILED!' -ForegroundColor Red
    throw
}
finally {
    Pop-Location
}