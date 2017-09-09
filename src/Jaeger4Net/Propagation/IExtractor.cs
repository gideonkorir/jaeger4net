using Jaeger4Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Propagation
{
    /// <summary>
    /// Extract the span context from the supplied carrier
    /// </summary>
    /// <typeparam name="T">The type of the carrier</typeparam>
    public interface IExtractor<T>
    {
        SpanContext Extract(T carrier);
    }
}
