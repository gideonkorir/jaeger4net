using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Jaeger4Net.Baggage
{
    public class RemoteRestrictorOptions
    {
        public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromMinutes(1);
        public string[] Services { get; set; }
        public bool DenyBaggageOnInitializationFailure { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
