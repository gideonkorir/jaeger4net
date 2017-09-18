using Jaeger4Net.Metrics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Baggage
{
    public class BaggageSetter
    {
        readonly IRestrictBaggage baggageRestrictor;
        readonly ClientMetrics metrics;

        public BaggageSetter(IRestrictBaggage baggageRestrictor, ClientMetrics metrics)
        {
            this.baggageRestrictor = baggageRestrictor ?? throw new ArgumentNullException(nameof(baggageRestrictor));
            this.metrics = metrics;
        }

        public SpanContext SetBaggage(Span span, string key, string value)
        {
            var restriction = baggageRestrictor.Get(span.ServiceName, key);
            string prevItem = null;
            bool truncated = false;

            if (!restriction.KeyAllowed)
            {
                metrics.BaggageUpdateFailure(delta: 1);
                LogFields(span, key, value, prevItem, truncated, restriction.KeyAllowed);
                return span.Context as SpanContext;
            }
            if (value.Length > restriction.MaxValueLength)
            {
                value = value.Substring(0, restriction.MaxValueLength);
                truncated = true;
                metrics.BaggageTruncate(delta: 1);
            }
            if (!span.TryGetBaggageItem(key, out prevItem))
                prevItem = null;
            LogFields(span, key, value, prevItem, truncated, restriction.KeyAllowed);
            metrics.BaggageUpdateSuccess(delta: 1);
            return (span.Context as SpanContext).AppendBaggageItem(key, value);
        }

        static void LogFields(Span span, string key, string value, string prevItem,
            bool truncated, bool valid)
        {
            if (!(span.Context as SpanContext).IsSampled)
                return;
            var map = new Dictionary<string, object>()
            {
                ["event"] = "baggage",
                ["key"] = key,
                ["value"] = value
            };
            if(!string.IsNullOrEmpty(prevItem))
            {
                map.Add("override", true);
            }
            if(truncated)
            {
                map.Add("truncated", true);
            }
            if(!valid)
            {
                map.Add("invalid", true);
            }
            span.Log(map);
        }
    }
}
