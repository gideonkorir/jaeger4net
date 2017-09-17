using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Jaeger4Net.Sampling
{
    public interface IRetrieveSamplingStrategy
    {
        Task<SamplingStrategyResponse> Get(string serviceName);
    }
}
