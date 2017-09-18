using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net
{
    public class LogData
    {
        public DateTimeOffset Instant { get; }
        public string Message { get; }
        public object PayLoad { get; }
        public IReadOnlyDictionary<string, object> Fields { get; }

        public LogData(DateTimeOffset instant, string message, object payload)
        {
            this.Instant = instant;
            this.Message = message;
            this.PayLoad = payload;
        }

        public LogData(DateTimeOffset instant, IReadOnlyDictionary<string, object> fields)
        {
            this.Instant = instant;
            this.Fields = fields;
        }

        public LogData(DateTimeOffset instant, IEnumerable<KeyValuePair<string, object>> fields)
        {
            this.Instant = instant;
            if(fields != null)
            {
                var map = new Dictionary<string, object>();
                map.AddRange(fields);
                Fields = map;
            }
        }
    }
}
