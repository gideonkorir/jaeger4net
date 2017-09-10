using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Baggage
{
    public class MaxLengthBaggageRestrictor : IRestrictBaggage
    {
        readonly int maxLength;

        public MaxLengthBaggageRestrictor(int maxLength = BaggageConstants.DefaultMaxValueLength)
        {
            this.maxLength = maxLength;
        }

        public Restriction Get(string service, string key)
        {
            return new Restriction(true, maxLength);
        }
    }
}
