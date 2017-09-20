using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jaeger4Net.Sampling
{
    [DebuggerDisplay("{samplingRate}")]
    public sealed class ProbabilisticSampler : ISampler
    {
        private readonly double samplingRate;
        private readonly long negativeSamplingBoundary, positiveSamplingBoundary;
        readonly IReadOnlyDictionary<string, object> tags = null;
        readonly ISamplerObserver observer;

        public double SamplingRate => samplingRate;

        public ProbabilisticSampler(double samplingRate)
            : this(samplingRate, null)
        {

        }

        internal ProbabilisticSampler(double samplingRate, ISamplerObserver observer)
        {
            if (samplingRate < 0.0 || samplingRate > 1.0)
                throw new ArgumentException("sampling rate must be between 0.0 & 1.0");
            this.samplingRate = samplingRate;
            positiveSamplingBoundary = (long)(long.MaxValue * samplingRate);
            negativeSamplingBoundary = (long)(long.MinValue * samplingRate);
            tags = new Dictionary<string, object>()
            {
                [Constants.SAMPLER_TYPE_TAG_KEY] = "probabilistic",
                [Constants.SAMPLER_PARAM_TAG_KEY] = samplingRate
            };
            this.observer = observer;
        }

        public SamplingStatus Sample(string operation, long traceId)
        {
            var sample = traceId > 0
                ? new SamplingStatus(traceId <= positiveSamplingBoundary, tags)
                : new SamplingStatus(traceId >= negativeSamplingBoundary, tags);
            observer?.OnSampled(this, operation, traceId, sample);
            return sample;
        }

        public bool Equals(ISampler other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (other is ProbabilisticSampler p)
                return samplingRate == p.samplingRate;
            return false;
        }

        public override bool Equals(object obj)
            => Equals(obj as ProbabilisticSampler);

        public override int GetHashCode()
            => samplingRate.GetHashCode();

        public void Dispose()
        {
            //nothing to do here
        }
    }
}
