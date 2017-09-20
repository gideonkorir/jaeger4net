using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jaeger4Net.Tests
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

    static class Ext
    {
        public static HttpResponseMessage RespondWith(this HttpRequestMessage requ, string content)
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                RequestMessage = requ,
                Content = new StringContent(content),
            };
            return response;
        }

        public static HttpResponseMessage RespondWith<T>(this HttpRequestMessage req, T value)
            => RespondWith(req, JsonConvert.SerializeObject(value));
    }
}
