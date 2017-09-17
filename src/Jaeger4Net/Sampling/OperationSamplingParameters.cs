using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Sampling
{
    public class OperationSamplingParameters
    {
        public double DefaultSamplingProbability { get; set; }
        public double DefaultLowerBoundTracesPerSecond { get; set; }

        public List<PerOperationSamplingParameters> PerOperationStrategies { get; set; }
    }
}
