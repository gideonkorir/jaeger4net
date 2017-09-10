using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Metrics
{
    public class ReporterStatsFactory : IStatsFactory
    {
        IStatsReporter reporter;

        public ReporterStatsFactory(IStatsReporter reporter)
        {
            this.reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
        }

        public Counter Counter(string name, params Tag[] tags)
            => (delta) => reporter.Counter(name, delta, tags);

        public Gauge Gauge(string name, params Tag[] tags)
            => (value) => reporter.Gauge(name, value, tags);

        public Timer Timer(string name, params Tag[] tags)
            => (duration) => reporter.Timer(name, duration, tags);
    }
}
