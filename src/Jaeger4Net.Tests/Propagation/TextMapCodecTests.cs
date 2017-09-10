using Jaeger4Net.Propagation;
using Moq;
using OpenTracing.Propagation;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Jaeger4Net.Tests.Propagation
{
    public class TextMapCodecTests
    {
        [Fact]
        public void Text_Codec_Injects_Context_AsString_To_Carrier()
        {
            var dict = new Dictionary<string, string>();
            var map = new Mock<ITextMap>();
            map.Setup(c => c.Set(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((c1, c2) => dict.Add(c1, c2));
            var codec = new TextMapCodec(false);
            codec.Inject(SpanContext.Parse("1:2:3:4"), map.Object);
            Assert.Single(dict);
            Assert.True(dict.ContainsKey(TextMapCodec.SPAN_CONTEXT_KEY));
            Assert.Equal("1:2:3:4", dict[TextMapCodec.SPAN_CONTEXT_KEY]);
        }

        [Fact]
        public void TextCodec_Injects_Context_AsString_Using_SuppliedKey()
        {
            var dict = new Dictionary<string, string>();
            var map = new Mock<ITextMap>();
            map.Setup(c => c.Set(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((c1, c2) => dict.Add(c1, c2));
            var codec = new TextMapCodec(false, contextKey: "context", baggagePrefix: "baggage");
            codec.Inject(SpanContext.Parse("1:2:3:4"), map.Object);
            Assert.Single(dict);
            Assert.True(dict.ContainsKey("context"));
            Assert.Equal("1:2:3:4", dict["context"]);
        }

        [Fact]
        public void TextMapCode_Injects_Extracts_Successful_Without_Encoding()
        {
            var dict = new Dictionary<string, string>();
            var map = new Mock<ITextMap>();
            map.Setup(c => c.Set(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((c1, c2) => dict.Add(c1, c2));
            map.Setup(c => c.GetEntries()).Returns(dict);

            var codec = new TextMapCodec(false);
            //inject is successful
            codec.Inject(SpanContext.Parse("1:2:3:4"), map.Object);
            Assert.Single(dict);
            Assert.True(dict.ContainsKey(TextMapCodec.SPAN_CONTEXT_KEY));
            Assert.Equal("1:2:3:4", dict[TextMapCodec.SPAN_CONTEXT_KEY]);

            //extract is successful
            var context = codec.Extract(map.Object);
            Assert.NotNull(context);
            Assert.Equal(1, context.TraceId);
            Assert.Equal(2, context.SpanId);
            Assert.Equal(3, context.ParentId);
            Assert.Equal(4, context.Flags);
            Assert.Empty(context.Baggage);
        }

        [Fact]
        public void TextMapCode_Injects_Extracts_Successful_With_Encoding()
        {
            var dict = new Dictionary<string, string>();
            var map = new Mock<ITextMap>();
            map.Setup(c => c.Set(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((c1, c2) => dict.Add(c1, c2));
            map.Setup(c => c.GetEntries()).Returns(dict);

            var codec = new TextMapCodec(true);
            //inject is successful
            codec.Inject(SpanContext.Parse("1:2:3:4"), map.Object);
            Assert.Single(dict);
            Assert.True(dict.ContainsKey(TextMapCodec.SPAN_CONTEXT_KEY));
            Assert.Equal("1%3A2%3A3%3A4", dict[TextMapCodec.SPAN_CONTEXT_KEY]);

            //extract is successful
            var context = codec.Extract(map.Object);
            Assert.NotNull(context);
            Assert.Equal(1, context.TraceId);
            Assert.Equal(2, context.SpanId);
            Assert.Equal(3, context.ParentId);
            Assert.Equal(4, context.Flags);
            Assert.Empty(context.Baggage);

        }

        [Fact]
        public void Text_Map_Injects_Extracts_Baggage_Without_Encoding()
        {
            var context = new SpanContext(9, 9, 0, 5, new Dictionary<string, string>
            {
                ["host"] = "test",
                ["ip"] = "::1"
            }, null);

            var dict = new Dictionary<string, string>();
            var map = new Mock<ITextMap>();
            map.Setup(c => c.Set(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((c1, c2) => dict.Add(c1, c2));
            map.Setup(c => c.GetEntries()).Returns(dict);

            var codec = new TextMapCodec(false);
            codec.Inject(context, map.Object);
            Assert.Equal("test", dict["host".WithPrefix(TextMapCodec.BAGGAGE_KEY_PREFIX)]);
            Assert.Equal("::1", dict["ip".WithPrefix(TextMapCodec.BAGGAGE_KEY_PREFIX)]);

            var extracted = codec.Extract(map.Object);
            Assert.Equal(2, extracted.Baggage.Count);
            Assert.Equal("test", extracted.Baggage["host"]);
            Assert.Equal("::1", extracted.Baggage["ip"]);
        }

        [Fact]
        public void Text_Map_Injects_Extracts_Baggage_With_Encoding()
        {
            var context = new SpanContext(9, 9, 0, 5, new Dictionary<string, string>
            {
                ["host"] = "test",
                ["ip"] = "::1"
            }, null);

            var dict = new Dictionary<string, string>();
            var map = new Mock<ITextMap>();
            map.Setup(c => c.Set(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((c1, c2) => dict.Add(c1, c2));
            map.Setup(c => c.GetEntries()).Returns(dict);

            var codec = new TextMapCodec(true);
            codec.Inject(context, map.Object);
            Assert.Equal("test", dict["host".WithPrefix(TextMapCodec.BAGGAGE_KEY_PREFIX)]);
            Assert.Equal("%3A%3A1", dict["ip".WithPrefix(TextMapCodec.BAGGAGE_KEY_PREFIX)]);

            var extracted = codec.Extract(map.Object);
            Assert.Equal(2, extracted.Baggage.Count);
            Assert.Equal("test", extracted.Baggage["host"]);
            Assert.Equal("::1", extracted.Baggage["ip"]);
        }

        [Fact]
        public void TextMapCodec_Override_Keys_Are_Respected()
        {
            var context = new SpanContext(9, 9, 0, 5, new Dictionary<string, string>
            {
                ["host"] = "test",
                ["ip"] = "::1"
            }, null);

            var dict = new Dictionary<string, string>();
            var map = new Mock<ITextMap>();
            map.Setup(c => c.Set(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((c1, c2) => dict.Add(c1, c2));
            map.Setup(c => c.GetEntries()).Returns(dict);

            var codec = new TextMapCodec(false, contextKey: "ctx", baggagePrefix: "bg");
            codec.Inject(context, map.Object);
            Assert.Equal("9:9:0:5", dict["ctx"]);
            Assert.Equal("test", dict["host".WithPrefix("bg")]);
            Assert.Equal("::1", dict["ip".WithPrefix("bg")]);

            var extracted = codec.Extract(map.Object);
            Assert.Equal(2, extracted.Baggage.Count);
            Assert.Equal("test", extracted.Baggage["host"]);
            Assert.Equal("::1", extracted.Baggage["ip"]);
        }
    }
}
