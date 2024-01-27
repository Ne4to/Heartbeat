$ErrorActionPreference = "Stop"

$RepositoryRoot = Split-Path $PSScriptRoot
$FrontendRoot = Join-Path $RepositoryRoot 'src/Heartbeat/ClientApp'
$ContractPath = Join-Path $FrontendRoot 'api.yml'
$DllPath = Join-Path $RepositoryRoot 'src/Heartbeat/bin/Debug/net8.0/Heartbeat.dll'

Push-Location
try
{
    Set-Location $RepositoryRoot

    dotnet tool restore
    dotnet build --configuration Debug
    
    Set-Location $FrontendRoot
    $env:HEARTBEAT_GENERATE_CONTRACTS = 'true'
    dotnet swagger tofile --yaml --output $ContractPath $DllPath Heartbeat
    dotnet kiota generate -l typescript --openapi $ContractPath -c HeartbeatClient -o ./src/client --clean-output

    # TODO try  --serializer  Microsoft.Kiota.Serialization.Json.JsonSerializationWriterFactory --deserializer Microsoft.Kiota.Serialization.Json.JsonParseNodeFactory
}
catch {
    Write-Host 'Generate client - FAILED!' -ForegroundColor Red
    throw
}
finally {
    Pop-Location
    $env:HEARTBEAT_GENERATE_CONTRACTS = $null
}