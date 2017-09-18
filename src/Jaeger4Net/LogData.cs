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
        public IEnumerable<KeyValuePair<string, object>> Fields { get; }

        public LogData(DateTimeOffset instant, string message, object payload)
        {
            this.Instant = instant;
            this.Message = message;
            this.PayLoad = payload;
        }

        public LogData(DateTimeOffset instant, IEnumerable<KeyValuePair<string, object>> fields)
        {
            this.Instant = instant;
            this.Fields = fields;
        }
    }
}
