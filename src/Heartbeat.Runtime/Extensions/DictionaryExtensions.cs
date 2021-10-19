using System.Collections.Generic;

namespace Heartbeat.Runtime.Extensions
{
    public static class DictionaryExtensions
    {
        public static void IncrementValue<TKey>(this IDictionary<TKey, int> workItemTypeCount, TKey key)
            where TKey : notnull
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
        
        public static void IncrementValue<TKey>(this IDictionary<TKey, long> workItemTypeCount, TKey key, long value)
            where TKey : notnull
        {
            if (workItemTypeCount.TryGetValue(key, out var currentValue))
            {
                workItemTypeCount[key] = currentValue + value;
            }
            else
            {
                workItemTypeCount[key] = value;
            }
        }        
    }
}