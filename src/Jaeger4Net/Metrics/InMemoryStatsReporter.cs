using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Metrics
{
    public class InMemoryStatsReporter : IStatsReporter
    {
        readonly ConcurrentDictionary<string, long> counters = new ConcurrentDictionary<string, long>();
        readonly ConcurrentDictionary<string, long> gauges = new ConcurrentDictionary<string, long>();
        readonly ConcurrentDictionary<string, TimeSpan> timers = new ConcurrentDictionary<string, TimeSpan>();
         
        public ConcurrentDictionary<string, long> Counters => counters;
        public ConcurrentDictionary<string, long> Gauges => gauges;
        public ConcurrentDictionary<string, TimeSpan> Timers => timers;

        public void Counter(string name, long delta, params Tag[] tags)
        {
            var newName = MetricNames.Format(name, tags);
            counters.AddOrUpdate(newName, 1, (c, d) => d + delta);
        }

        public void Gauge(string name, long value, params Tag[] tags)
        {
            var newName = MetricNames.Format(name, tags);
            gauges.AddOrUpdate(newName, value, (c, d) => value);
        }

        public void Timer(string name, TimeSpan duration, params Tag[] tags)
        {
            var newName = MetricNames.Format(name, tags);
            timers.AddOrUpdate(newName, duration, (c, d) => d + duration);
        }
    }
}
