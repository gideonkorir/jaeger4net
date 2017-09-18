using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Sampling
{
    /// <summary>
    /// Retrieved response from <see cref="IRetrieveSamplingStrategy.Get(string)"/>
    /// </summary>
    public class SamplingStrategyResponse
    {
        public ProbabilisticSamplingParameter ProbabilisticSampling { get; set; }
        public RateLimitingSamplingParameter RateLimitingSampling { get; set; }
        public OperationSamplingParameters OperationSampling { get; set; }
    }

    public class OperationSamplingParameters
    {
        public double DefaultSamplingProbability { get; set; }
        public double DefaultLowerBoundTracesPerSecond { get; set; }

        public List<PerOperationSamplingParameters> PerOperationStrategies { get; set; }
    }

    public class RateLimitingSamplingParameter
    {
        public double MaxTracesPerSecond { get; set; }
    }

    public class PerOperationSamplingParameters
    {
        public string Operation { get; set; }
        public ProbabilisticSamplingParameter ProbabilisticSampling { get; set; }
    }
    public class ProbabilisticSamplingParameter
    {
        public double SamplingRate { get; set; }
    }
}
