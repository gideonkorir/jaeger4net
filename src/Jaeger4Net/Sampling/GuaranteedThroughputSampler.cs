using Jaeger4Net.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Jaeger4Net.Sampling
{
    /// <summary>
    /// Guarantees throughput by using a <see cref="ProbabilisticSampler"/> and <see cref="RateLimitingSampler"/>.
    /// 
    /// <see cref="RateLimitingSampler"/> is used to establish a lower bound so that every operation is sampled
    /// at least once in the time interval defined by the lower bound.
    /// </summary>
    public class GuaranteedThroughputSampler : ISampler
    {
        ProbabilisticSampler probabilisticSampler;
        RateLimitingSampler rateLimitingSampler;
        readonly Dictionary<string, object> tags;
        readonly IClock clock;

        /// <summary>
        /// The sampling rate for the probabilistic sampler
        /// </summary>
        public double SamplingRate
        {
            get => probabilisticSampler.SamplingRate;
            set
            {
                //update the sampling rate
                if (value != probabilisticSampler.SamplingRate)
                {
                    var probSampler = new ProbabilisticSampler(value);
                    ProbabilisticSampler copy, replaced;
                    do
                    {
                        copy = probabilisticSampler;
                        replaced = Interlocked.CompareExchange(ref probabilisticSampler, probSampler, copy);
                    }
                    while (copy != replaced);
                }
                lock (tags)
                    tags[Constants.SAMPLER_PARAM_TAG_KEY] = value;
            }
        }

        /// <summary>
        /// The lower bound for the rate limiting sampler
        /// </summary>
        public double LowerBound
        {
            get => rateLimitingSampler.MaxTracesPerSecond;
            set
            {
                if (value != rateLimitingSampler.MaxTracesPerSecond)
                {
                    var rateSampler = new RateLimitingSampler(value, clock);
                    RateLimitingSampler copy, replaced;
                    do
                    {
                        copy = rateLimitingSampler;
                        replaced = Interlocked.CompareExchange(ref rateLimitingSampler, rateSampler, copy);
                    }
                    while (copy != replaced);
                }
            }
        }

        public GuaranteedThroughputSampler(double samplingRate, double lowerBound, IClock clock)
        {
            probabilisticSampler = new ProbabilisticSampler(samplingRate);
            rateLimitingSampler = new RateLimitingSampler(lowerBound, clock);
            this.clock = clock;
            tags = new Dictionary<string, object>()
            {
                [Constants.SAMPLER_TYPE_TAG_KEY] = "lowerbound",
                [Constants.SAMPLER_PARAM_TAG_KEY] = samplingRate
            };
        }


        /// <summary>
        /// Calls <see cref="ISampler.Sample(string, long)" on both samplers 
        /// <see cref="ProbabilisticSampler"/>/> and <see cref="RateLimitingSampler"/>
        /// returning true if either samplers <see cref="SamplingStatus.Sampled"/> == true. 
        /// The tags corresponding to the sampler that returned true are set on the 
        /// <see cref="SamplingStatus.Tags"/>.If both samplers return true, tags for 
        /// <see cref="ProbabilisticSampler"/> is given priority.
        /// </summary>
        /// <param name="operation">The operation name, ignored by this sampler</param>
        /// <param name="traceId">The traceId on the span</param>
        /// <returns></returns>
        public SamplingStatus Sample(string operation, long traceId)
        {
            var probSampler = probabilisticSampler;
            var rateSampler = rateLimitingSampler; //clr guarantees thread safety

            var probabilitySample = probSampler.Sample(operation, traceId);
            if (probabilitySample)
                return probabilitySample;

            var rateSample = rateSampler.Sample(operation, traceId);
            return new SamplingStatus(rateSample, tags);
        }

        public bool Equals(ISampler other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (other is GuaranteedThroughputSampler g)
                return probabilisticSampler.Equals(g.probabilisticSampler)
                    && rateLimitingSampler.Equals(g.rateLimitingSampler);
            return false;
        }

        public override bool Equals(object obj)
            => Equals(obj as GuaranteedThroughputSampler);

        public override int GetHashCode()
            => base.GetHashCode();

        public void Dispose()
        {
            probabilisticSampler.Dispose();
            rateLimitingSampler.Dispose();
        }
    }
}
