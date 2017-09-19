using Jaeger4Net.Metrics;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Jaeger4Net.Tests.Metrics
{
    public class MetricsTest
    {
        [Fact]
        public void TestCounterWithoutExplicitTags()
        {
            var reporter = new InMemoryStatsReporter();
            var metrics = new ClientMetrics(reporter);
            metrics.DecodingErrors(delta: 1);
            Assert.True(reporter.Counters.TryGetValue("jaeger.decoding-errors", out var value));
            Assert.Equal(1L, value);
            Assert.Single(reporter.Counters);
        }

        [Fact]
        public void TestCounterWithExplicitTags()
        {
            var reporter = new InMemoryStatsReporter();
            var metrics = new ClientMetrics(reporter);
            metrics.TracesJoinedSampled(delta: 1);
            Assert.True(reporter.Counters.TryGetValue("jaeger.traces.sampled=y.state=joined", out var value));
            Assert.Equal(1L, value);
            Assert.Single(reporter.Counters);
        }
        
        [Fact]
        public void TestGaugeWithoutExplicitTags()
        {
            var reporter = new InMemoryStatsReporter();
            var metrics = new ClientMetrics(reporter);
            metrics.ReporterQueueLength(value: 1);
            Assert.True(reporter.Gauges.TryGetValue("jaeger.reporter-queue", out var value));
            Assert.Equal(1L, value);
            Assert.Single(reporter.Gauges);
        }
    }
}
