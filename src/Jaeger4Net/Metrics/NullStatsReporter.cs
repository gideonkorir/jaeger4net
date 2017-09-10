using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Metrics
{
    public class NullStatsReporter : IStatsReporter
    {
        public void Counter(string name, long delta, params Tag[] tags)
        {
        }

        public void Gauge(string name, long value, params Tag[] tags)
        {
        }

        public void Timer(string name, TimeSpan duration, params Tag[] tags)
        {
        }
    }
}
