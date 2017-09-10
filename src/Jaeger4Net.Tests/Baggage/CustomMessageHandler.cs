using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jaeger4Net.Tests.Baggage
{
    class CustomMessageHandler : HttpMessageHandler
    {
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler;

        public CustomMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            this.handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return handler(request, cancellationToken);
        }
    }
}
