namespace Heartbeat.Runtime.Domain;

[Flags]
public enum ObjectGCStatus
{
  Live,
  Dead
}