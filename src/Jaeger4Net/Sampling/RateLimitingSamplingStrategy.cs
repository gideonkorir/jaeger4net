using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Sampling
{
    public class RateLimitingSamplingStrategy
    {
        public double MaxTracesPerSecond { get; set; }
    }
}
