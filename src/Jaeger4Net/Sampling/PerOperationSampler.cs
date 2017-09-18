using Jaeger4Net.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Sampling
{
    class PerOperationSampler : ISampler
    {
        static readonly ILogger<PerOperationSampler> log = Log.Create<PerOperationSampler>();

        readonly object objLock = new object();
        readonly int maxOperations;
        readonly Dictionary<string, GuaranteedThroughputSampler> operationSamplers;
        readonly IClock clock;
        ProbabilisticSampler probabilisticSampler;
        double lowerBound;

        public PerOperationSampler(int maxOperations, OperationSamplingParameters operationSamplingParameters,
            IClock clock)
        {
            this.maxOperations = maxOperations;
            operationSamplers = new Dictionary<string, GuaranteedThroughputSampler>();
            probabilisticSampler = new ProbabilisticSampler(operationSamplingParameters.DefaultSamplingProbability);
            lowerBound = operationSamplingParameters.DefaultLowerBoundTracesPerSecond;
            this.clock = clock;
        }

        public bool Update(OperationSamplingParameters operationSamplingParameters)
        {
            bool updated = false;
            var pSampler = new ProbabilisticSampler(operationSamplingParameters.DefaultSamplingProbability);

            lock(objLock)
            {
                lowerBound = operationSamplingParameters.DefaultLowerBoundTracesPerSecond;
                if(probabilisticSampler.Equals(pSampler))
                {
                    probabilisticSampler = pSampler;
                    updated = true;
                }
                foreach(var strategy in operationSamplingParameters.PerOperationStrategies)
                {
                    if(operationSamplers.TryGetValue(strategy.Operation, out var sampler))
                    {
                        updated = sampler.Update(
                            strategy.ProbabilisticSampling.SamplingRate,
                            lowerBound
                            ) || updated;
                    }
                    else
                    {
                        if(operationSamplers.Count < maxOperations)
                        {
                            sampler = new GuaranteedThroughputSampler(
                                strategy.ProbabilisticSampling.SamplingRate,
                                lowerBound,
                                clock
                                );
                            operationSamplers.Add(strategy.Operation, sampler);
                            updated = true;
                        }
                        else
                        {
                            log.LogWarning(
                                "Exceeded the number of operations {operations} for per operation sampling",
                                maxOperations
                                );
                        }
                    }

                }

            }
            return updated;
        }

        public SamplingStatus Sample(string operation, long traceId)
        {
            GuaranteedThroughputSampler sampler = null;
            bool found = false;
            int count = 0;
            lock(objLock)
            {
                found = operationSamplers.TryGetValue(operation, out sampler);
                count = operationSamplers.Count; //we never remove samplers
            }

            if (found)
                return sampler.Sample(operation, traceId);

            if(count > maxOperations)
            {
                sampler = new GuaranteedThroughputSampler(
                    probabilisticSampler.SamplingRate,
                    lowerBound,
                    clock
                    );
                return sampler.Sample(operation, traceId);
            }

            return probabilisticSampler.Sample(operation, traceId);
        }

        public bool Equals(ISampler other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if(other is PerOperationSampler op)
            {
                if (lowerBound != op.lowerBound)
                    return false;
                else if (!probabilisticSampler.Equals(op.probabilisticSampler))
                    return false;
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            lock(objLock)
            {
                probabilisticSampler.Dispose();
                foreach (var value in operationSamplers.Values)
                    value.Dispose();
            }
        }
    }
}
