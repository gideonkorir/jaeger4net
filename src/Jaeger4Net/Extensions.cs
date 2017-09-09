using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net
{
    static class Extensions
    {
        public static void Add<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, KeyValuePair<TKey, TValue> valuePair)
        {
            dictionary.Add(valuePair.Key, valuePair.Value);
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
        {
            if (keyValuePairs == null)
                return;
            foreach (var kvp in keyValuePairs)
                dictionary.Add(kvp.Key, kvp.Value);
        }
    }
}
