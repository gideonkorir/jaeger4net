using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Sampling
{
    /// <summary>
    /// The outcome of sampling + tags associated with the sampling process
    /// </summary>
    public struct SamplingStatus
    {
        public bool Sampled { get; }
        public IReadOnlyDictionary<string, object> Tags { get; }

        public SamplingStatus(bool sampled, IReadOnlyDictionary<string, object> tags)
        {
            Sampled = sampled;
            Tags = tags;
        }

        public static implicit operator bool(SamplingStatus status)
        {
            return status.Sampled;
        }

        public static implicit operator SamplingStatus(bool value)
        {
            return new SamplingStatus(value, null);
        }
    }
}
