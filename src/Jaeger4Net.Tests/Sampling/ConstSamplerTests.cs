using Jaeger4Net.Sampling;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Jaeger4Net.Tests.Sampling
{
    public class ConstSamplerTests
    {
        [Fact]
        public void TestConstSamplerTrue()
        {
            var sampler = new ConstSampler(true);
            var status = sampler.Sample("any", 9L);
            Assert.True(status);
            Assert.Equal("const", status.Tags[Constants.SAMPLER_TYPE_TAG_KEY]);
            Assert.True(status.Tags[Constants.SAMPLER_PARAM_TAG_KEY].Equals(true));
        }

        [Fact]
        public void TestConstSamplerFalse()
        {
            var sampler = new ConstSampler(false);
            var status = sampler.Sample("any", 9L);
            Assert.False(status);
            Assert.Equal("const", status.Tags[Constants.SAMPLER_TYPE_TAG_KEY]);
            Assert.True(status.Tags[Constants.SAMPLER_PARAM_TAG_KEY].Equals(false));
        }
    }
}
