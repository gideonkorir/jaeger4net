using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Jaeger4Net.Tests
{
    public class SpanContextTests
    {
        [Theory]
        [InlineData("1:1:0:9", 1, 1, 0, 9)]
        [InlineData("209:98:987:255", 209, 98, 987, 255)]
        public void SpanContext_Parses_Correctly_Formatted_String(string value, int traceId, int spanId,
            int parentId, byte flags)
        {
            var parsed = SpanContext.TryParse(value, out var context);
            Assert.True(parsed);
            Assert.Equal(traceId, context.TraceId);
            Assert.Equal(spanId, context.SpanId);
            Assert.Equal(parentId, context.ParentId);
            Assert.Equal(flags, context.Flags);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("98:9:7")]
        [InlineData("a:b:8:1")]
        [InlineData("98:98:97:689")] //flag must be byte
        public void SpanContext_Parse_For_Malformed_String(string value)
        {
            Assert.False(SpanContext.TryParse(value, out var _));
        }
    }
}
