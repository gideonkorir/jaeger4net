using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Metrics
{
    public interface IStatsReporter
    {
        void Counter(string name, long delta, params Tag[] tags);

        void Gauge(string name, long value, params Tag[] tags);

        void Timer(string name, TimeSpan duration, params Tag[] tags);
    }
}
