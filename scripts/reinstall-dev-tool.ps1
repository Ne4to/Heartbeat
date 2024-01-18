$ErrorActionPreference = "Stop"

$RepositoryRoot = Split-Path $PSScriptRoot

Push-Location
try
{
    Set-Location $RepositoryRoot

    dotnet tool uninstall -g Heartbeat
    dotnet clean --configuration Release
    Get-Date -Format ''
    $VersionSuffix = "rc.$(Get-Date -Format 'yyyy-MM-dd-HHmm')"
    dotnet pack --version-suffix $VersionSuffix
    $PackageVersion = "0.1.0-$VersionSuffix"
    dotnet tool install --global --add-source ./src/Heartbeat/nupkg Heartbeat --version $PackageVersion
}
catch {
    Write-Host 'Install global tool - FAILED!' -ForegroundColor Red
    throw
}
finally {
    Pop-Location
}