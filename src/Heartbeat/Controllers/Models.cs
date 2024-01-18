using Microsoft.Diagnostics.Runtime;

namespace Heartbeat.Host.Controllers;

// ReSharper disable NotAccessedPositionalProperty.Global
public record ObjectTypeStatistics(ulong MethodTable, string TypeName, ulong TotalSize, int InstanceCount);

public record GetObjectInstancesResult(
    ulong MethodTable,
    string? TypeName,
    IReadOnlyList<ObjectInstance> Instances);

public record ObjectInstance(ulong Address, ulong Size);

public record GetClrObjectResult(
    string? ModuleName,
    string? TypeName,
    ulong MethodTable,
    ulong Size,
    IReadOnlyList<ClrObjectField> Fields);

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