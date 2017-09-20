using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Jaeger4Net.Sampling
{
    public class HttpSamplingStrategyRetriever : IRetrieveSamplingStrategy
    {
        private readonly HttpClient client;
        readonly HostPort hostPort;

        public HttpSamplingStrategyRetriever(HttpClient client, HostPort hostPort)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.hostPort = hostPort;
        }
        public async Task<SamplingStrategyResponse> Get(string serviceName)
        {
            var path = $"http://{hostPort.Host}:{hostPort.Port}/?service={WebUtility.UrlEncode(serviceName)}";
            try
            {
                var content = await client.GetStringAsync(path)
                    .ConfigureAwait(false);
                return ParseJson(content);
            }
            catch(Exception ex) when (!(ex is ParseException))
            {
                throw new SamplingStrategyRetrieveException("Could not retrieve sampling strategy", ex);
            }
        }

        SamplingStrategyResponse ParseJson(string json)
        {
            try
            {
                return (SamplingStrategyResponse)JsonConvert.DeserializeObject(json, typeof(SamplingStrategyResponse));
            }
            catch(Exception ex)
            {
                throw new ParseException($"Could not parse json: {json}", ex, json);
            }
        }
    }
}
