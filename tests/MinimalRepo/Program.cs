using Microsoft.Diagnostics.Runtime;

const string dumpPath = "/Users/ne4to/projects/dbg/dumps/coredump.37588";
using var dataTarget = DataTarget.LoadDump(dumpPath);
var clrInfo = dataTarget.ClrVersions.Single();
using var clrRuntime = clrInfo.CreateRuntime();
Console.WriteLine("Counting roots...");
var rootCount = clrRuntime.Heap.EnumerateRoots().Count();
Console.WriteLine($"Roots: {rootCount}");
