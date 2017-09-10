using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jaeger4Net.Baggage
{
    public class HttpRestrictionSource : IRestrictionSource
    {
        readonly HttpClient client;
        readonly HostPort host;
        readonly IResponseDeserializer deserializer;

        public HttpRestrictionSource(HttpClient client, HostPort host)
            : this(client, host, new JsonResponseSerializer())
        {

        }

        public HttpRestrictionSource(HttpClient client, HostPort host, IResponseDeserializer deserializer)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.host = host;
            this.deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
        }

        public async Task<IList<RestrictionResponse>> FetchAsync(string serviceName, CancellationToken cancellationToken)
        {
            var uri = GetEndPoint(serviceName);
            var content = await client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
            var str = await content.Content.ReadAsStringAsync().ConfigureAwait(false);
            var parsed = deserializer.Parse(str);
            return new List<RestrictionResponse>(parsed);
        }

        protected virtual Uri GetEndPoint(string serviceName)
            => new Uri($"http://{host.Host}:{host.Port}//baggageRestrictions?service={serviceName}");
    }
}
