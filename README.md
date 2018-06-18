# Heartbeat purpose

The purpose of the Heartbeat is finding runtime issues of .Net application in the production environment such as spontaneous high memory / CPU usage, high latency and so on.
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
- Attach .Net invasive debugger and collect .Net specific information (ClrMd).

Issue Finder example:
- Find a place with huge memory allocation;
- Find hot stack traces;
- Find hung System.Threading.Tasks.Task objects;
- Find System.Threading.Tasks.Task state.
