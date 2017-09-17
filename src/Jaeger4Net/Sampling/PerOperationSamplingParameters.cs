using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Sampling
{
    public class PerOperationSamplingParameters
    {
        public string Operation { get; set; }
        public ProbabilisticSamplingStrategy ProbabilisticSampling { get; set; }
    }
}
