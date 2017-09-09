using Jaeger4Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Propagation
{
    /// <summary>
    /// Injects span context into the carrier.
    /// </summary>
    /// <typeparam name="T">The type of the carrier</typeparam>
    public interface IInjector<T>
    {
        void Inject(SpanContext context, T carrier);
    }
}
