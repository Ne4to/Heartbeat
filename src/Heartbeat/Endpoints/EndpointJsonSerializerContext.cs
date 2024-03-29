﻿using System.Text.Json.Serialization;

namespace Heartbeat.Host.Endpoints;

[JsonSourceGenerationOptions(UseStringEnumConverter = true)]
[JsonSerializable(typeof(DumpInfo))]
[JsonSerializable(typeof(GetObjectInstancesResult))]
[JsonSerializable(typeof(GetClrObjectResult))]
[JsonSerializable(typeof(JwtInfo))]
[JsonSerializable(typeof(DictionaryInfo))]
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
[JsonSerializable(typeof(IEnumerable<ClrObjectArrayItem>))]
[JsonSerializable(typeof(IEnumerable<HttpRequestInfo>))]
internal partial class EndpointJsonSerializerContext : JsonSerializerContext;