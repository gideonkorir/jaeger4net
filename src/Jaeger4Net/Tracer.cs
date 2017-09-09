using OpenTracing;
using System;
using System.Collections.Generic;
using System.Text;
using OpenTracing.Propagation;
using Jaeger4Net.Jaeger.Utils;

namespace Jaeger4Net.Jaeger
{
    public class Tracer : ITracer
    {
        public string ServiceName { get; }

        public IClock Clock { get; }
        public ISpanBuilder BuildSpan(string operationName)
        {
            throw new NotImplementedException();
        }

        public ISpanContext Extract<TCarrier>(Format<TCarrier> format, TCarrier carrier)
        {
            throw new NotImplementedException();
        }

        public void Inject<TCarrier>(ISpanContext spanContext, Format<TCarrier> format, TCarrier carrier)
        {
            throw new NotImplementedException();
        }

        public void Report(Span span)
        {
            throw new NotImplementedException();
        }
    }
}
