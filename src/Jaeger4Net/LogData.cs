using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Jaeger
{
    public class LogData
    {
        readonly DateTimeOffset instant;
        readonly string message;
        readonly object payload;
        readonly IEnumerable<KeyValuePair<string, object>> fields;

        public LogData(DateTimeOffset instant, string message, object payload)
        {
            this.instant = instant;
            this.message = message;
            this.payload = payload;
        }

        public LogData(DateTimeOffset instant, IEnumerable<KeyValuePair<string, object>> fields)
        {
            this.instant = instant;
            this.fields = fields;
        }
    }
}
