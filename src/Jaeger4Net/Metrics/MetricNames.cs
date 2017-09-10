using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Metrics
{
    static class MetricNames
    {
        public static string Format(string userSuppliedName, Tag[] tags)
        {
            if (tags == null || tags.Length == 0)
                return userSuppliedName;

            var builder = new StringBuilder(userSuppliedName);
            //order the keys then concat them using .key=value
            var set = new SortedSet<Tag>(tags);
            foreach(var item in set)
            {
                builder.Append('.');
                builder.Append(item.Key);
                builder.Append('=');
                builder.Append(item.Value);
            }
            return builder.ToString();
        }
    }
}
