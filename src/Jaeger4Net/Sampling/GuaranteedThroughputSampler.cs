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
        readonly object objLock = new object();

        ProbabilisticSampler probabilisticSampler;
        RateLimitingSampler rateLimitingSampler;
        readonly Dictionary<string, object> tags;
        readonly IClock clock;

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
            ProbabilisticSampler probSampler = null;
            RateLimitingSampler rateSampler = null;
            lock (objLock)
            {
                probSampler = probabilisticSampler;
                rateSampler = rateLimitingSampler;
            }

            var probabilitySample = probSampler.Sample(operation, traceId);
            if (probabilitySample)
                return probabilitySample;

            var rateSample = rateSampler.Sample(operation, traceId);
            return new SamplingStatus(rateSample, tags);
        }

        /// <summary>
        /// Updates either the samplingRate, lowerBound or both.
        /// Returns true if either of them has changed.
        /// NB: Upates tags if the sampling rate is different
        /// </summary>
        /// <param name="samplingRate">The probabilistic sampling rate</param>
        /// <param name="lowerBound">Rate limiting lower bound</param>
        /// <returns></returns>
        public bool Update(double samplingRate, double lowerBound)
        {
            bool updated = false;
            if(samplingRate != probabilisticSampler.SamplingRate)
            {
                var pSampler = new ProbabilisticSampler(samplingRate);
                lock(objLock)
                {
                    probabilisticSampler = pSampler;
                    tags[Constants.SAMPLER_PARAM_TAG_KEY] = samplingRate;
                }
                updated = true;
            }
            if(lowerBound != rateLimitingSampler.MaxTracesPerSecond)
            {
                var rSampler = new RateLimitingSampler(lowerBound, clock);
                lock(objLock)
                {
                    rateLimitingSampler = rSampler;
                }
                updated = true;
            }
            return updated;
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
            lock (objLock)
            {
                probabilisticSampler.Dispose();
                rateLimitingSampler.Dispose();
            }
        }
    }
}
