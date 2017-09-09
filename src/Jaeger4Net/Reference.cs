using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Jaeger
{
    public class Reference
    {
        public SpanContext Context { get; }
        public string Type { get; }

        public Reference(SpanContext context, string type)
        {
            this.Context = context;
            this.Type = type;
        }
    }
}
