$ErrorActionPreference = "Stop"

$RepositoryRoot = Split-Path $PSScriptRoot
$FrontendRoot = Join-Path $RepositoryRoot 'src/Heartbeat.WebUI/ClientApp'
$ContractPath = Join-Path $FrontendRoot 'api.yml'
$DllPath = Join-Path $RepositoryRoot 'src/Heartbeat.WebUI/bin/Debug/net8.0/Heartbeat.WebUI.dll'

Push-Location
try
{
    Set-Location $RepositoryRoot

    dotnet tool restore
    dotnet build --configuration Debug
    
    Set-Location $FrontendRoot
    dotnet swagger tofile --yaml --output $ContractPath $DllPath Heartbeat
    dotnet kiota generate -l typescript --openapi $ContractPath -c HeartbeatClient -o ./src/client

    # TODO try  --serializer  Microsoft.Kiota.Serialization.Json.JsonSerializationWriterFactory --deserializer Microsoft.Kiota.Serialization.Json.JsonParseNodeFactory
}
catch {
    Write-Host 'Generate client - FAILED!' -ForegroundColor Red
    throw
}
finally {
    Pop-Location
}