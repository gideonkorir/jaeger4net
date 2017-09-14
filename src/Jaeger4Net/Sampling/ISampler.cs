using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Sampling
{
    public interface ISampler : IDisposable, IEquatable<ISampler>
    {
        /// <summary>
        /// the operation name set on the span
        /// the traceid on the span
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        SamplingStatus Sample(string operation, long traceId);
    }
}
