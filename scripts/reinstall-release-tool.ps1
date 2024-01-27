$ErrorActionPreference = "Stop"

try
{
    dotnet tool uninstall -g Heartbeat
    dotnet tool install --global Heartbeat
}
catch {
    Write-Host 'Install global tool - FAILED!' -ForegroundColor Red
    throw
}