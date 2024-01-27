using Heartbeat.Runtime.Domain;

namespace Heartbeat.Runtime.Analyzers.Interfaces;

public interface IWithObjectGCStatus
{
    ObjectGCStatus? ObjectGcStatus { get; set; }
}