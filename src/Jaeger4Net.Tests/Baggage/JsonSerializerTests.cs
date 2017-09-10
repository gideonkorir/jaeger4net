using Jaeger4Net.Baggage;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Jaeger4Net.Tests.Baggage
{
    public class JsonSerializerTests
    {
        [Fact]
        public void Serializer_Deserializes_Array()
        {
            var json = @"[
                { ""key"":""abc"", ""maxvaluelength"":""243"" },
                { ""key"":""test"", ""maxvaluelength"":""56"" }
            ]";
            var serializer = new JsonResponseSerializer();
            var responses = serializer.Parse(json);
            Assert.Equal(2, responses.Length);
            Assert.Equal("abc", responses[0].Key);
            Assert.Equal(243, responses[0].MaxValueLength);
            Assert.Equal("test", responses[1].Key);
            Assert.Equal(56, responses[1].MaxValueLength);
        }
    }
}
