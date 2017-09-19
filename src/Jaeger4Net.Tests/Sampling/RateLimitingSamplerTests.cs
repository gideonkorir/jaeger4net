using Jaeger4Net.Sampling;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Jaeger4Net.Tests.Sampling
{
    /// <summary>
    /// RateLimitingSampler internally uses a <see cref="Jaeger4Net.Utils.RateLimiter"/>
    /// so no need to test much because we have tests in
    /// <seealso cref="Utils.RateLimiterTests"/>
    /// </summary>
    public class RateLimitingSamplerTests
    {
        [Fact]
        public void TestRateLimitingTags()
        {
            var sampler = new RateLimitingSampler(1.0, new Jaeger4Net.Utils.CoreClrClock());
            var status = sampler.Sample("what", 98L);
            Assert.Equal("ratelimiting", status.Tags[Constants.SAMPLER_TYPE_TAG_KEY]);
            Assert.Equal(1.0, (double)status.Tags[Constants.SAMPLER_PARAM_TAG_KEY]);
        }
    }
}
