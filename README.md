# Heartbeat
Diagnostics utility to analyze memory dumps of a .NET application
[![NuGet Badge](https://buildstats.info/nuget/heartbeat?includePreReleases=true&dWidth=0)](https://www.nuget.org/packages/Heartbeat/)

## Getting started

```shell
dotnet tool install --global Heartbeat
heartbeat --dump <path-to-dump-file>
```
Open `http://localhost:5000/` in web browser.
See [UI screen](https://github.com/Ne4to/Heartbeat/tree/master/assets) for examples

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
## Usage

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
<!---
TODO: add screens
-->
