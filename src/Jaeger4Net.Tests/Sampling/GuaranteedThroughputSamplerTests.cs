using Jaeger4Net.Sampling;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Jaeger4Net.Tests.Sampling
{
    public class GuaranteedThroughputSamplerTests
    {
        [Fact]
        public void TestRateLimitingLowerBound()
        {
            var sampler = new GuaranteedThroughputSampler(0.0001, 1.0, new Jaeger4Net.Utils.CoreClrClock());
            //in this case we will be sampled by the rate limiting sampler
            //because the traceId is outside our probability range
            var status = sampler.Sample("test-me", long.MaxValue);
            Assert.True(status);
            Assert.Equal("lowerbound", status.Tags[Constants.SAMPLER_TYPE_TAG_KEY]);
            Assert.Equal(0.0001, (double)status.Tags[Constants.SAMPLER_PARAM_TAG_KEY]);
        }

        [Fact]
        public void TestProbabilityTagsOverrideRateLimitingTags()
        {
            var sampler = new GuaranteedThroughputSampler(0.999, 1.0, new Jaeger4Net.Utils.CoreClrClock());
            //we will be sampled by the probabilistic sampler
            //because traceId is within the probability range
            var status = sampler.Sample("test-me", 0L);
            Assert.True(status);
            Assert.Equal("probabilistic", status.Tags[Constants.SAMPLER_TYPE_TAG_KEY]);
            Assert.Equal(0.999, (double)status.Tags[Constants.SAMPLER_PARAM_TAG_KEY]);
        }

        [Fact]
        public void TestUpdateProbabilisticSampler()
        {
            var sampler = new GuaranteedThroughputSampler(0.001, 1, new Jaeger4Net.Utils.CoreClrClock());

            Assert.False(sampler.Update(0.001, 1));
            Assert.True(sampler.Update(0.002, 1));
            //we will be sampled by the RateLimitingSampler
            //and we get default tags
            var status = sampler.Sample("test", long.MaxValue);
            Assert.True(status);

            Assert.Equal("lowerbound", status.Tags[Constants.SAMPLER_TYPE_TAG_KEY]);
            Assert.Equal(0.002, (double)status.Tags[Constants.SAMPLER_PARAM_TAG_KEY]);
        }

        [Fact]
        public void TestUpdateRateLimitingSampler()
        {
            var sampler = new GuaranteedThroughputSampler(0.001, 1, new Jaeger4Net.Utils.CoreClrClock());

            Assert.False(sampler.Update(0.001, 1));
            Assert.True(sampler.Update(0.001, 0));
            //we will be sampled by the ProbabilisticSampler
            //and we get probability tags
            var status = sampler.Sample("test", 0L);
            Assert.True(status);

            Assert.Equal("probabilistic", status.Tags[Constants.SAMPLER_TYPE_TAG_KEY]);
            Assert.Equal(0.001, (double)status.Tags[Constants.SAMPLER_PARAM_TAG_KEY]);
        }
    }
}
