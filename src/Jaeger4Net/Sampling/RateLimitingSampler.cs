using Jaeger4Net.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Sampling
{
    public class RateLimitingSampler : ISampler
    {
        readonly double maxTracesPerSecond;
        readonly RateLimiter rateLimiter;
        readonly IReadOnlyDictionary<string, object> tags;

        public double MaxTracesPerSecond => maxTracesPerSecond;

        public RateLimitingSampler(double maxTracesPerSecond, IClock clock)
        {
            this.maxTracesPerSecond = maxTracesPerSecond;
            double maxBalance = maxTracesPerSecond < 1.0 ? 1.0 : maxTracesPerSecond;
            rateLimiter = new RateLimiter(maxTracesPerSecond, maxBalance, clock);

            tags = new Dictionary<string, object>()
            {
                [Constants.SAMPLER_TYPE_TAG_KEY] = "ratelimiting",
                [Constants.SAMPLER_PARAM_TAG_KEY] = maxTracesPerSecond
            };
        }
        public SamplingStatus Sample(string operation, long traceId)
        {
            return new SamplingStatus(rateLimiter.CheckCredit(1), tags);
        }

        public bool Equals(ISampler other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (other is RateLimitingSampler r)
                return maxTracesPerSecond == r.maxTracesPerSecond;
            return false;
        }

        public override bool Equals(object obj)
            => Equals(obj as RateLimitingSampler);

        public override int GetHashCode()
            => maxTracesPerSecond.GetHashCode();

        public void Dispose()
        {
        }
    }
}
