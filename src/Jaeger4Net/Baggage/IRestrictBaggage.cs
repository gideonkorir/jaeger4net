using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Baggage
{
    public interface IRestrictBaggage
    {
        Restriction Get(string service, string key);
    }

    internal static class BaggageConstants
    {
        public const int DefaultMaxValueLength = 2048;
    }
}
