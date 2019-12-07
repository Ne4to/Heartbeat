using System;
using Humanizer;

namespace Heartbeat.Runtime.Models
{
    public class StringDuplicate
    {
        public string Value { get; }
        public string ShortValue { get; }
        public int InstanceCount { get; }

        public StringDuplicate(string value, int instanceCount)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            ShortValue = Value.Truncate(80);
            InstanceCount = instanceCount;
        }
    }
}