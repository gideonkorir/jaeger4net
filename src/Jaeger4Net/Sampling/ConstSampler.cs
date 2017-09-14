using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Jaeger4Net.Sampling
{
    [DebuggerDisplay("{decision}")]
    class ConstSampler : ISampler
    {
        readonly bool decision = false;
        readonly IReadOnlyDictionary<string, object> tags;

        public ConstSampler(bool decision)
        {
            this.decision = decision;
            tags = new Dictionary<string, object>()
            {
                [Constants.SAMPLER_TYPE_TAG_KEY] = "const",
                [Constants.SAMPLER_PARAM_TAG_KEY] = decision
            };
        }

        public SamplingStatus Sample(string operation, long traceId)
            => new SamplingStatus(decision, tags);

        public bool Equals(ISampler other)
        {
            if (ReferenceEquals(other, null))
                return false;
            if (other is ConstSampler c)
                return decision == c.decision;
            return false;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ConstSampler);
        }

        public override int GetHashCode()
            => decision.GetHashCode();
        public void Dispose()
        {
        }
    }
}
