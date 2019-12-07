using System.Collections.Generic;

namespace Heartbeat.Runtime.Extensions
{
    public static class DictionaryExtensions
    {
        public static void IncrementValue<TKey>(this IDictionary<TKey, int> workItemTypeCount, TKey key)
        {
            if (workItemTypeCount.TryGetValue(key, out var currentValue))
            {
                workItemTypeCount[key] = currentValue + 1;
            }
            else
            {
                workItemTypeCount[key] = 1;
            }
        }
    }
}