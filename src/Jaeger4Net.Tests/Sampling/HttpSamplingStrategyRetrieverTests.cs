using Jaeger4Net.Sampling;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Jaeger4Net.Tests.Sampling
{
    public class HttpSamplingStrategyRetrieverTests
    {
        const string validstring = @"
            { 
                ""probabilisticsampling"": { ""samplingrate"":""0.878"" },
                ""ratelimitingsampling"": { ""maxtracespersecond"": ""0.65"" },
                ""operationsampling"": { 
                        ""defaultsamplingprobability"": ""0.76"",
                        ""defaultlowerboundtracespersecond"": ""0.2"",
                        ""peroperationstrategies"": [ 
                                { ""operation"": ""some-op"", ""probabilisticsampling"": { ""samplingrate"": ""0.76"" } },
                                { ""operation"": ""opcode2"", ""probabilisticsampling"": { ""samplingrate"": ""0.21"" } }
                          ]
                }
            }
          ";

        [Fact]
        public async Task TestGetCallsCorrectUri()
        {
            Uri uri = null;
            var handler = new CustomMessageHandler((c, d) =>
            {
                uri = c.RequestUri;
                var response = c.RespondWith(validstring);
                return Task.FromResult(response);
            });
            var retriever = new HttpSamplingStrategyRetriever(new HttpClient(handler), new HostPort("localhost", 987));
            var resp = await retriever.Get("some service");
            Assert.Equal(new Uri("http://localhost:987/?service=some+service"), uri);
        }

        [Fact]
        public async Task TestGetValidResult()
        {
            Uri uri = null;
            var handler = new CustomMessageHandler((c, d) =>
            {
                uri = c.RequestUri;
                var response = c.RespondWith(validstring);
                return Task.FromResult(response);
            });
            var retriever = new HttpSamplingStrategyRetriever(new HttpClient(handler), new HostPort("localhost", 987));
            var resp = await retriever.Get("whoop");
            Assert.Equal(0.878, resp.ProbabilisticSampling.SamplingRate);
            Assert.Equal(0.65, resp.RateLimitingSampling.MaxTracesPerSecond);
            Assert.Equal(0.76, resp.OperationSampling.DefaultSamplingProbability);
            Assert.Equal(0.2, resp.OperationSampling.DefaultLowerBoundTracesPerSecond);
            Assert.Equal(2, resp.OperationSampling.PerOperationStrategies.Count);
            Assert.Equal("some-op", resp.OperationSampling.PerOperationStrategies[0].Operation);
            Assert.Equal(0.76, resp.OperationSampling.PerOperationStrategies[0].ProbabilisticSampling.SamplingRate);
            Assert.Equal("opcode2", resp.OperationSampling.PerOperationStrategies[1].Operation);
            Assert.Equal(0.21, resp.OperationSampling.PerOperationStrategies[1].ProbabilisticSampling.SamplingRate);
        }

        [Fact]
        public async Task TestThrowsInvalidJson()
        {
            Uri uri = null;
            var handler = new CustomMessageHandler((c, d) =>
            {
                uri = c.RequestUri;
                var response = c.RespondWith("something not json");
                return Task.FromResult(response);
            });
            var retriever = new HttpSamplingStrategyRetriever(new HttpClient(handler), new HostPort("localhost", 987));
            var ex = await Record.ExceptionAsync(() => retriever.Get("someservice"));
            Assert.NotNull(ex);
            Assert.IsType<ParseException>(ex);
        }
    }
}
