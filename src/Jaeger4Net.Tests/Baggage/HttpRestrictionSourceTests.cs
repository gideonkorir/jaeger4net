using Jaeger4Net.Baggage;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jaeger4Net.Tests.Baggage
{
    public class HttpRestrictionSourceTests
    {

        [Fact]
        public async Task Correct_Endpoint_Is_Invoked()
        {
            Uri actual = null;
            var handler = new CustomMessageHandler((r, c) =>
            {
                actual = r.RequestUri;
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(@"[ {""key"":""k"", ""maxvaluelength"": ""2"" } ]")
                });
            });
            var source = new HttpRestrictionSource(
                new HttpClient(handler),
                new HostPort("localhost", 876),
                new JsonResponseSerializer()
                );
            var items = await source.FetchAsync("matrix", default(CancellationToken));
            var expected = new Uri("http://localhost:876/baggageRestrictions?service=matrix");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task Service_Name_Is_Encoded()
        {
            Uri actual = null;
            var handler = new CustomMessageHandler((r, c) =>
            {
                actual = r.RequestUri;
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(@"[ {""key"":""k"", ""maxvaluelength"": ""2"" } ]")
                });
            });
            var source = new HttpRestrictionSource(
                new HttpClient(handler),
                new HostPort("localhost", 876),
                new JsonResponseSerializer()
                );
            var items = await source.FetchAsync("a:b", default(CancellationToken));
            var expected = new Uri("http://localhost:876/baggageRestrictions?service=a%3Ab");
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task Response_Content_Is_Parsed_Correctly()
        {
            var handler = new CustomMessageHandler((r, c) =>
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        @"[ {""key"":""k"", ""maxvaluelength"": ""2"" }, { ""key"":""home"", ""maxvaluelength"":""365"" } ]"
                    )
                });
            });
            var source = new HttpRestrictionSource(
                new HttpClient(handler),
                new HostPort("localhost", 876),
                new JsonResponseSerializer()
                );
            var items = await source.FetchAsync("parser", default(CancellationToken));
            Assert.Equal(2, items.Count);
            Assert.Equal("k", items[0].Key);
            Assert.Equal(2, items[0].MaxValueLength);
            Assert.Equal("home", items[1].Key);
            Assert.Equal(365, items[1].MaxValueLength);
        }
    }
}
