using Jaeger4Net.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Sampling
{
    public sealed class PerOperationSampler : ISampler
    {
        static readonly ILogger<PerOperationSampler> log = Log.Create<PerOperationSampler>();

        readonly object objLock = new object();
        readonly int maxOperations;
        readonly Dictionary<string, GuaranteedThroughputSampler> operationSamplers;
        readonly IClock clock;
        ProbabilisticSampler probabilisticSampler;
        double lowerBound;

        readonly ISamplerObserver observer;

        //exists for testing
        internal IReadOnlyDictionary<string, GuaranteedThroughputSampler> OperationToSamplers => operationSamplers;

        public PerOperationSampler(int maxOperations, OperationSamplingParameters operationSamplingParameters,
            IClock clock)
            : this(maxOperations, operationSamplingParameters, clock, null)
        {
            
        }

        internal PerOperationSampler(int maxOperations, OperationSamplingParameters operationSamplingParameters,
            IClock clock, ISamplerObserver observer)
        {
            this.maxOperations = maxOperations;
            operationSamplers = new Dictionary<string, GuaranteedThroughputSampler>(maxOperations);
            probabilisticSampler = new ProbabilisticSampler(operationSamplingParameters.DefaultSamplingProbability, observer);
            lowerBound = operationSamplingParameters.DefaultLowerBoundTracesPerSecond;
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            AddStrategies(operationSamplers, operationSamplingParameters, lowerBound, clock, observer);
            this.observer = observer;
        }

        public bool Update(OperationSamplingParameters operationSamplingParameters)
        {
            bool updated = false;
            var pSampler = new ProbabilisticSampler(operationSamplingParameters.DefaultSamplingProbability, observer);

            lock(objLock)
            {
                lowerBound = operationSamplingParameters.DefaultLowerBoundTracesPerSecond;
                if(!probabilisticSampler.Equals(pSampler))
                {
                    probabilisticSampler = pSampler;
                    updated = true;
                    observer?.OnSamplingRateUpdated(pSampler, pSampler.SamplingRate);
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
                                clock,
                                observer
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
            OperationSamplingParameters samplingParameters, double lowerBound, IClock clock, ISamplerObserver observer)
        {
            var toAdd = samplingParameters.PerOperationStrategies.ConvertAll
                (
                c => new KeyValuePair<string, GuaranteedThroughputSampler>(
                    c.Operation,
                    new GuaranteedThroughputSampler(c.ProbabilisticSampling.SamplingRate, lowerBound, clock, observer)
                    )
                );
            target.AddRange(toAdd);
        }
    }
}
