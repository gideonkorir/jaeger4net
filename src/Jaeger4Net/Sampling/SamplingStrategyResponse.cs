using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Sampling
{
    public class SamplingStrategyResponse
    {
        public ProbabilisticSamplingStrategy ProbabilisticSampling { get; set; }
        public RateLimitingSamplingStrategy RateLimitingSampling { get; set; }
        public OperationSamplingParameters OperationSampling { get; set; }
    }
}
