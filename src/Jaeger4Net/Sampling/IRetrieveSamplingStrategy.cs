using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Jaeger4Net.Sampling
{
    /// <summary>
    /// Used to remotely get the sampling strategy.
    /// See <see cref="https://eng.uber.com/distributed-tracing/"/>
    /// </summary>
    public interface IRetrieveSamplingStrategy
    {
        Task<SamplingStrategyResponse> Get(string serviceName);
    }
}
