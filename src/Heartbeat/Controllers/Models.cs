using Microsoft.Diagnostics.Runtime;

using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Heartbeat.Host.Controllers;

// ReSharper disable NotAccessedPositionalProperty.Global
public record DumpInfo(
    string DumpPath,
    bool CanWalkHeap,
    bool IsServerHeap,
    string ClrModulePath,
    Architecture Architecture,
    int ProcessId,
    string Platform,
    string RuntimeVersion);

public record ObjectTypeStatistics(ulong MethodTable, string TypeName, ulong TotalSize, int InstanceCount);

public record GetObjectInstancesResult(
    ulong MethodTable,
    string? TypeName,
    IReadOnlyList<ObjectInstance> Instances);

public record ObjectInstance(ulong Address, ulong Size);

public record GetClrObjectResult(
    ulong Address,
    string? ModuleName,
    string? TypeName,
    ulong MethodTable,
    ulong Size,
    Generation Generation,
    string? Value);

public record ClrObjectField(
    ulong MethodTable,
    string? TypeName,
    int Offset,
    bool IsValueType,
    ulong? ObjectAddress,
    string Value,
    string Name);

public record Module(ulong Address, ulong Size, string? Name);

public record HeapSegment(ulong Start, ulong End, GCSegmentKind Kind)
{
    public ulong Size => End - Start;
}

public record StringInfo(ulong Address, int Length, ulong Size, string Value);

public record StringDuplicate(string Value, int Count, int FullLength, ulong WastedMemory);

public record RootInfo(ulong Address, ClrRootKind Kind, bool IsPinned, ulong Size, ulong MethodTable, string TypeName);

public record ClrObjectRootPath(RootInfo Root, IReadOnlyList<RootPathItem> PathItems);

public record RootPathItem(ulong Address, ulong MethodTable, string? TypeName, ulong Size, Generation Generation);

public record ArrayInfo(ulong Address, ulong MethodTable, string? TypeName, int Length, int UnusedItemsCount, double UnusedPercent, ulong Wasted);

public record SparseArrayStatistics(ulong MethodTable, string? TypeName, int Count, ulong TotalWasted);

[JsonSourceGenerationOptions(UseStringEnumConverter = true)]
[JsonSerializable(typeof(DumpInfo))]
[JsonSerializable(typeof(GetObjectInstancesResult))]
[JsonSerializable(typeof(GetClrObjectResult))]
[JsonSerializable(typeof(Module[]))]
[JsonSerializable(typeof(ClrObjectField[]))]
[JsonSerializable(typeof(List<ClrObjectRootPath>))]
[JsonSerializable(typeof(IEnumerable<HeapSegment>))]
[JsonSerializable(typeof(IEnumerable<IEnumerable<RootInfo>>))]
[JsonSerializable(typeof(IEnumerable<ObjectTypeStatistics>))]
[JsonSerializable(typeof(IEnumerable<StringInfo>))]
[JsonSerializable(typeof(IEnumerable<StringDuplicate>))]
[JsonSerializable(typeof(IEnumerable<ArrayInfo>))]
[JsonSerializable(typeof(IEnumerable<SparseArrayStatistics>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}