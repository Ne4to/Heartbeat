# Heartbeat
[![NuGet Badge](https://buildstats.info/nuget/heartbeat?includePreReleases=false&dWidth=0)](https://www.nuget.org/packages/Heartbeat/)

Diagnostics utility with web UI to analyze .NET application memory dump

## Getting started

### dotnet tool

.NET 8 SDK is required

```shell
dotnet tool install --global Heartbeat
# optional
export PATH=$PATH:$HOME/.dotnet/tools
heartbeat --dump <path-to-dump-file>
```
Open `http://localhost:5000/` in web browser.

### Native AOT
Heartbeat is also available in [Native AOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/) version. You can download it from the [latest release](https://github.com/Ne4to/Heartbeat/releases/latest)

<!---
TODO: update description
## Summary

The purpose of the Heartbeat is finding runtime issues of .NET application in the production environment such as spontaneous high memory / CPU usage, high latency and so on.
Usually such situations occur unpredictable and after that there is not enough information to find the cause.
The detailed information is required to find the cause, but it is impossible to collect detailed information about running processes all the time, collecting such data consumes huge resources.
The collected data needs to be interpreted, it is long and not always straightforward process that could be automated.

## Overview
The system consists of three major parts: Monitor, Data Collector and Issue Finder:

Monitor example:
- CPU usage;
- RAM usage;
- Performance counter;
- I/O latency;
- Events from application code;
- Timers from application code.

Data Collector example:
- Full Memory Dump;
- Record ETW events;
- Record network events;
- Attach .NET invasive debugger and collect .NET specific information (ClrMd).

Issue Finder example:
- Find a place with huge memory allocation;
- Find hot stack traces;
- Find hung System.Threading.Tasks.Task objects;
- Find System.Threading.Tasks.Task state.
-->
### Using Heartbeat

```
heartbeat [options]

Options:
  --dump <dump> (REQUIRED)  Path to a dump file
  --dac-path <dac-path>     A full path to the matching DAC dll for this
                            process.
  --ignore-dac-mismatch     Ignore mismatches between DAC versions
  --version                 Show version information
  -?, -h, --help            Show help and usage information
```

### Features
<!-- do not change to relative path, file included in NuGet readme it works only with absolute path -->
See [Features](https://github.com/Ne4to/Heartbeat/blob/master/docs/features.md) for more info.

### Listening endpoint

Use [ASPNETCORE_URLS](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-8.0) environment variable to change default endpoint (`export ASPNETCORE_URLS=http://0.0.0.0:5555` or `$env:ASPNETCORE_URLS='http://127.0.0.1:5555'`)
