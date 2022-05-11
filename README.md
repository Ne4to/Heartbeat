# Heartbeat
Diagnostics utility to analyze memory dumps of a .NET application

## Installation
[![NuGet Badge](https://buildstats.info/nuget/heartbeat?includePreReleases=true&dWidth=0)](https://www.nuget.org/packages/Heartbeat/)
```
dotnet tool install --global Heartbeat --version <version>
```

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

## Usage

```
Heartbeat [options]

Options:
  -pid, --process-id <pid>    Process Id
  --dump <dump>               Path to a dump file
  --heap                      Print heap information
  --service-point-manager     Print System.Net.ServicePointManager information
  --async-state-machine       Print System.Runtime.CompilerServices.IAsyncStateMachine information
  --long-string               Print long System.String objects
  --string-duplicate          Print System.String duplicates
  --task                      Print System.Threading.Tasks.Task objects
  --timer-queue-timer         Print System.Threading.TimerQueueTimer information
  --task-completion-source    Print System.Threading.Tasks.TaskCompletionSource objects
  --object-type-statistics    Print heap object type statistics
  --http-client               Print System.Net.Http.HttpClient objects
  --version                   Show version information
  -?, -h, --help              Show help and usage information
```