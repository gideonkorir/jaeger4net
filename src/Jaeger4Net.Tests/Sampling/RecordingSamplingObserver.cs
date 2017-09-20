using Jaeger4Net.Sampling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Tests.Sampling
{
    class RecordingSamplingObserver : ISamplerObserver
    {
        public Dictionary<ISampler, double> LowerBounds { get; } = new Dictionary<ISampler, double>(new SamplerRefComparer());
        public Dictionary<ISampler, double> SamplingRates { get; } = new Dictionary<ISampler, double>(new SamplerRefComparer());
        public Dictionary<ISampler, TraceInfo> SamplesBySampler { get; } = new Dictionary<ISampler, TraceInfo>(new SamplerRefComparer());

        public Dictionary<string, TraceInfo> SamplesByOperation { get; } = new Dictionary<string, TraceInfo>();
        
        public void OnLowerBoundUpdated(ISampler sampler, double lowerBound)
        {
            LowerBounds[sampler] = lowerBound;
        }

        public void OnSampled(ISampler sampler, string operation, long traceId, SamplingStatus outcome)
        {
            var info = new TraceInfo()
            {
                Operation = operation,
                TraceId = traceId,
                Status = outcome,
                Sampler = sampler
            };
            SamplesBySampler[sampler] = info;
            SamplesByOperation[operation] = info;
        }

        public void OnSamplingRateUpdated(ISampler sampler, double samplingRate)
        {
            SamplingRates[sampler] = samplingRate;
        }
    }

    public class TraceInfo
    {
        public string Operation { get; set; }
        public long TraceId { get; set; }
        public SamplingStatus Status { get; set; }
        public ISampler Sampler { get; internal set; }
    }

    public class SamplerRefComparer : IEqualityComparer<ISampler>
    {
        public bool Equals(ISampler x, ISampler y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(ISampler obj)
        {
            return obj.GetHashCode();
        }
    }
}
