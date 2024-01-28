$ErrorActionPreference = "Stop"

function Get-Runtime {
    if ($IsWindows) {
        return $env:PROCESSOR_ARCHITECTURE -eq 'AMD64' ? 'win-x64' : 'win-arm64'
    }

    if ($IsLinux) {
        return (uname -m) -eq 'aarch64' ? 'linux-arm64' : 'linux-x64'
    }

    if ($IsMacOS) {
        return (uname -m) -eq 'arm64' ? 'osx-arm64' : 'osx-x64'
    }
}

function Assert-ExitCode {
    if (-not $?) {
        throw 'Latest command failed'
    }
}

$Configuration = 'ReleaseAOT'
$RepositoryRoot = Split-Path $PSScriptRoot
$SpaRoot = Join-Path $RepositoryRoot 'src/Heartbeat/ClientApp'
$PublishProject = Join-Path $RepositoryRoot 'src/Heartbeat/Heartbeat.csproj'
$ArtifactsRoot = Join-Path $RepositoryRoot 'artifacts'

Push-Location
try {
    $SpaRoot
    Set-Location $SpaRoot
    npm install
    npm run build

    Set-Location $RepositoryRoot

    # [xml]$XmlConfig = Get-Content 'Directory.Build.props'

    # $XmlElement = Select-Xml '/Project/PropertyGroup/VersionPrefix' $XmlConfig |
    # Select-Object -ExpandProperty Node

    # $VersionPrefix = $XmlElement.InnerText
    # $VersionSuffix = "rc.$(Get-Date -Format 'yyyy-MM-dd-HHmm')"
    # $PackageVersion = "$VersionPrefix-$VersionSuffix"

    # dotnet clean --configuration $Configuration

    # https://learn.microsoft.com/en-us/dotnet/core/rid-catalog
    # $Runtimes = @('win-x64', 'win-arm64', 'linux-x64', 'linux-arm64', 'osx-x64', 'osx-arm64')
    $Runtimes = @(Get-Runtime)
    # TODO check `uname -m` for linux and macos
    # $env:PROCESSOR_ARCHITECTURE
    foreach ($Runtime in $Runtimes) {
        $OutDir = Join-Path $ArtifactsRoot $Runtime
        Write-Host "Publish native AOT version for $Runtime to $OutDir"
        dotnet publish --configuration $Configuration --runtime $Runtime --output $OutDir $PublishProject
        Assert-ExitCode

        Write-Host "Files in $OutDir"
        Get-ChildItem $OutDir | Select-Object -Property Length,Name
    }

    # TODO zip?
}
catch {
    Write-Host 'Publish AOT - FAILED!' -ForegroundColor Red
    throw
}
finally {
    Pop-Location
}

# TODO Cross-OS native compilation
# C:\Users\Ne4to\.nuget\packages\microsoft.dotnet.ilcompiler\8.0.1\build\Microsoft.NETCore.Native.Windows.targets(123,5): error : Platform linker not found. Ensure you have all the required prerequisites documented at https://aka.ms/nativeaot-prerequisites, in particular the Desktop Development for C++ workload in Visual Studio. For ARM64 development also install C++ ARM64 build tools. [C:\Users\Ne4to\projects\github.com\Ne4to\Heartbeat\src\Heartbeat\Heartbeat.csproj]
# C:\Users\Ne4to\.nuget\packages\microsoft.dotnet.ilcompiler\8.0.1\build\Microsoft.NETCore.Native.Publish.targets(60,5): error : Cross-OS native compilation is not supported. [C:\Users\Ne4to\projects\github.com\Ne4to\Heartbeat\src\Heartbeat\Heartbeat.csproj]