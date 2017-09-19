using Jaeger4Net.Baggage;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Jaeger4Net.Tests.Baggage
{
    public class MaxLengthBaggageRestrictorTests
    {
        [Fact]
        public void TestGetRestriction()
        {
            var restrictor = new MaxLengthBaggageRestrictor();
            var key = "key";
            var actual = restrictor.Get(string.Empty, key);
            Assert.Equal(new Restriction(true, 2048), actual);
        }
    }
}
