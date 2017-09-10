using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Baggage
{
    public class BaggageSetter
    {
        readonly IRestrictBaggage baggageRestrictor;

        public BaggageSetter(IRestrictBaggage baggageRestrictor)
        {
            this.baggageRestrictor = baggageRestrictor ?? throw new ArgumentNullException(nameof(baggageRestrictor));
        }

        public SpanContext SetBaggage(Span span, string key, string value)
        {
            var restriction = baggageRestrictor.Get(span.ServiceName, key);
            string prevItem = null;
            if (!restriction.KeyAllowed)
            {
                //todo:update metrics and log fields
                return span.Context as SpanContext;
            }
            if (value.Length > restriction.MaxValueLength)
            {
                value = value.Substring(0, restriction.MaxValueLength);
                //todo: update metrics
            }
            prevItem = span.GetBaggageItem(key);
            //log and update metrics
            return (span.Context as SpanContext).AppendBaggageItem(key, value);
        }
    }
}
