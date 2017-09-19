using Jaeger4Net.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Sampling
{
    public class PerOperationSampler : ISampler
    {
        static readonly ILogger<PerOperationSampler> log = Log.Create<PerOperationSampler>();

        readonly object objLock = new object();
        readonly int maxOperations;
        readonly Dictionary<string, GuaranteedThroughputSampler> operationSamplers;
        readonly IClock clock;
        ProbabilisticSampler probabilisticSampler;
        double lowerBound;

        //exists for testing
        internal IReadOnlyDictionary<string, GuaranteedThroughputSampler> OperationToSamplers => operationSamplers;

        public PerOperationSampler(int maxOperations, OperationSamplingParameters operationSamplingParameters,
            IClock clock)
        {
            this.maxOperations = maxOperations;
            operationSamplers = new Dictionary<string, GuaranteedThroughputSampler>(maxOperations);
            probabilisticSampler = new ProbabilisticSampler(operationSamplingParameters.DefaultSamplingProbability);
            lowerBound = operationSamplingParameters.DefaultLowerBoundTracesPerSecond;
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            AddStrategies(operationSamplers, operationSamplingParameters, lowerBound, clock);
        }

        
        internal PerOperationSampler(int maxOperations, OperationSamplingParameters samplingParameters,
            ProbabilisticSampler sampler, IClock clock)
        {
            this.maxOperations = maxOperations;
            operationSamplers = new Dictionary<string, GuaranteedThroughputSampler>(maxOperations);
            probabilisticSampler = sampler ?? throw new ArgumentNullException(nameof(sampler));
            lowerBound = samplingParameters.DefaultLowerBoundTracesPerSecond;
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            AddStrategies(operationSamplers, samplingParameters, lowerBound, clock);
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
                if(!operationSamplers.TryGetValue(operation, out sampler) && operationSamplers.Count < maxOperations)
                {
                    //add a sampler
                    sampler = new GuaranteedThroughputSampler(
                    probabilisticSampler.SamplingRate,
                    lowerBound,
                    clock
                    );
                    operationSamplers.Add(operation, sampler);
                }
            }

            return sampler != null 
                ? sampler.Sample(operation, traceId)
                : probabilisticSampler.Sample(operation, traceId);
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

        static void AddStrategies(Dictionary<string, GuaranteedThroughputSampler> target,
            OperationSamplingParameters samplingParameters, double lowerBound, IClock clock)
        {
            var toAdd = samplingParameters.PerOperationStrategies.ConvertAll
                (
                c => new KeyValuePair<string, GuaranteedThroughputSampler>(
                    c.Operation,
                    new GuaranteedThroughputSampler(c.ProbabilisticSampling.SamplingRate, lowerBound, clock)
                    )
                );
            target.AddRange(toAdd);
        }
    }
}
