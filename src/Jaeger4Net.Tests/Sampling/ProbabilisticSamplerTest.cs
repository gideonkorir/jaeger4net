using Jaeger4Net.Sampling;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Jaeger4Net.Tests.Sampling
{
    public class ProbabilisticSamplerTest
    {
        [Fact]
        public void TestSamplingBoundariesPositive()
        {
            const double samplingRate = 0.5;
            long halfwayBoundary = long.MaxValue / 2;

            var sampler = new ProbabilisticSampler(samplingRate);
            Assert.True(sampler.Sample("some-op", halfwayBoundary));
            Assert.False(sampler.Sample("another-op", halfwayBoundary + 2));
        }

        [Fact]
        public void TestSamplingBoundariesNegative()
        {
            const double samplingRate = 0.5;
            long halfwayBoundary = long.MinValue / 2;

            var sampler = new ProbabilisticSampler(samplingRate);
            Assert.True(sampler.Sample("some-op", halfwayBoundary));
            Assert.False(sampler.Sample("another-op", halfwayBoundary - 1));
        }

        [Fact]
        public void TestSamplerThrowsInvalidSamplingRangeExceptionUnder()
        {
            var ex = Record.Exception(() => new ProbabilisticSampler(-0.1));
            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public void TestSamplerThrowsInvalidSampingRangeExceptionOver()
        {
            var ex = Record.Exception(() => new ProbabilisticSampler(1.1));
            Assert.NotNull(ex);
            Assert.IsType<ArgumentException>(ex);
        }

        [Fact]
        public void TestTags()
        {
            var sampler = new ProbabilisticSampler(0.1);
            var status = sampler.Sample("vadcurry", 20L);
            Assert.Equal("probabilistic", status.Tags[Constants.SAMPLER_TYPE_TAG_KEY]);
            Assert.Equal(0.1, (double)status.Tags[Constants.SAMPLER_PARAM_TAG_KEY]);
        }
    }
}
