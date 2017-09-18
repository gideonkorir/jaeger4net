using Jaeger4Net.Baggage;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Jaeger4Net.Tests.Baggage
{
    public class BaggageSetterTests
    {
        readonly BaggageSetter setter;
        readonly Span span;
        readonly Mock<IRestrictBaggage> restrictorMock;
        readonly Metrics.InMemoryStatsReporter reporter;
        readonly Tracer tracer;

        public BaggageSetterTests()
        {
            reporter = new Metrics.InMemoryStatsReporter();
            var metrics = new Metrics.ClientMetrics(reporter);
            restrictorMock = new Mock<IRestrictBaggage>();

            tracer = new Tracer
                (
                "some-service",
                new Reporters.InMemoryReporter(),
                new Sampling.ConstSampler(true),
                new PropagationRegistry(),
                new Jaeger4Net.Utils.CoreClrClock(),
                metrics,
                new Dictionary<string, object>(),
                restrictorMock.Object
                );
            setter = new BaggageSetter(restrictorMock.Object, metrics);
            span = new SpanBuilder(tracer, "some-operation")
                .Start() as Span;
                
        }

        [Fact]
        public void TestInvalidBaggage()
        {
            restrictorMock.Setup(c => c.Get("some-service", "key"))
                .Returns(Restriction.Invalid);
            var ctx = setter.SetBaggage(span, "key", "value");

            Assert.False(ctx.Baggage.TryGetValue("key", out var _));
            AssertBaggageLogs(span, "key", "value", false, false, true);
            Assert.Equal(1L, reporter.Counters["jaeger.baggage-update.result=err"]);
        }

        [Fact]
        public void TestTruncatedBaggage()
        {
            restrictorMock.Setup(c => c.Get("some-service", "key"))
                .Returns(new Restriction(true, 5));
            const string actual = "0123456789";
            const string expected = "01234";

            var context = setter.SetBaggage(span, "key", actual);
            AssertBaggageLogs(span, "key", expected, true, false, false);

            Assert.Equal(expected, context.Baggage["key"]);

            Assert.Equal(1L, reporter.Counters["jaeger.baggage-truncate"]);
            Assert.Equal(1L, reporter.Counters["jaeger.baggage-update.result=ok"]);
        }

        [Fact]
        public void TestOverrideBaggage()
        {
            restrictorMock.Setup(c => c.Get("some-service", "key"))
                .Returns(new Restriction(true, 5));
            const string value = "value";
            var context = setter.SetBaggage(span, "key", value);
            var child = tracer.BuildSpan("some-child").AsChildOf(context).Start();
            using (child)
            {
                context = setter.SetBaggage(child as Span, "key", value);
                AssertBaggageLogs(child as Span, "key", "value", false, true, false);
                Assert.Equal(value, context.Baggage["key"]);
            }
            Assert.Equal(2L, reporter.Counters["jaeger.baggage-update.result=ok"]);

        }

        [Fact]
        public void TestUnsampledSpan()
        {
            var metrics = new Metrics.ClientMetrics(reporter);
            Span.Current = null; //this took me a while to debug
            var _tracer = new Tracer
                (
                "some-service2",
                new Reporters.InMemoryReporter(),
                new Sampling.ConstSampler(false),
                new PropagationRegistry(),
                new Jaeger4Net.Utils.CoreClrClock(),
                metrics,
                new Dictionary<string, object>(),
                restrictorMock.Object
                );
            var _span = _tracer.BuildSpan("some-operation")
                .Start() as Span;

            Assert.False((_span.Context as SpanContext).IsSampled);
            restrictorMock.Setup(c => c.Get("some-service2", "key"))
                .Returns(new Restriction(true, 5));
            var ctx = setter.SetBaggage(_span, "key", "value");
            Assert.Equal("value", ctx.Baggage["key"]);
            Assert.Empty(_span.Logs);
        }

        void AssertBaggageLogs(Span span, string key, string value,
            bool truncate, bool @override, bool invalid)
        {
            Assert.NotEmpty(span.Logs);
            var fields = span.Logs[span.Logs.Count - 1].Fields;
            Assert.Equal("baggage", fields["event"]);
            Assert.Equal(key, fields["key"]);
            Assert.Equal(value, fields["value"]);
            if(truncate)
            {
                Assert.True(fields["truncated"].Equals(true));
            }
            if(@override)
            {
                Assert.True(fields["override"].Equals(true));
            }
            if(invalid)
            {
                Assert.True(fields["invalid"].Equals(true));
            }
        }
    }
}
