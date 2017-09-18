using OpenTracing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using OpenTracing.Propagation;
using Jaeger4Net.Utils;
using Jaeger4Net.Reporters;
using Jaeger4Net.Sampling;
using Jaeger4Net.Metrics;
using Jaeger4Net.Baggage;
using Microsoft.Extensions.Logging;

namespace Jaeger4Net
{
    public class Tracer : ITracer
    {
        readonly ILogger log = Log.Create<Tracer>();

        public string ServiceName { get; }
        public IReporter Reporter { get; }
        public ISampler Sampler { get; }
        public IPropagationRegistry PropagationRegistry { get; }
        public IClock Clock { get; }
        public ClientMetrics Metrics { get; }
        public IReadOnlyDictionary<string, object> Tags { get; }
        public IRestrictBaggage BaggageRestrictor { get; }

        public Tracer(
            string serviceName,
            IReporter reporter,
            ISampler sampler,
            IPropagationRegistry propagationRegistry,
            IClock clock,
            ClientMetrics metrics,
            IDictionary<string, object> tags,
            IRestrictBaggage baggageRestrictor
            )
        {
            ServiceName = serviceName;
            Reporter = reporter;
            Sampler = sampler;
            PropagationRegistry = propagationRegistry;
            Clock = clock;
            Metrics = metrics;
            Tags = new Dictionary<string, object>(tags)
            {
                [Constants.JAEGER_CLIENT_VERSION_TAG_KEY] = "getversion",
                [Constants.TRACER_HOSTNAME_TAG_KEY] = Dns.GetHostName(),
                [Constants.TRACER_IP_TAG_KEY] = GetIpAddress()
            };
            BaggageRestrictor = baggageRestrictor;
        }

        public ISpanBuilder BuildSpan(string operationName)
        {
            return new SpanBuilder(this, operationName);
        }

        public ISpanContext Extract<TCarrier>(Format<TCarrier> format, TCarrier carrier)
        {
            if(PropagationRegistry.TryGetExtractor<TCarrier>(format, out var extractor))
            {
                return extractor.Extract(carrier);
            }
            var text = $"Extractor for type {typeof(TCarrier).Name} not found";
            log.LogWarning(text);
            throw new UnsupportedFormatException(text);
        }

        public void Inject<TCarrier>(ISpanContext spanContext, Format<TCarrier> format, TCarrier carrier)
        {
            if(!PropagationRegistry.TryGetInjector<TCarrier>(format, out var injector))
            {
                injector.Inject((SpanContext)spanContext, carrier);
                return;
            }
            var text = $"Injector for type {typeof(TCarrier).Name} not found";
            log.LogWarning(text);
            throw new UnsupportedFormatException(text);
            
        }

        public void Report(Span span)
        {
            Reporter.Report(span);
            Metrics.SpansFinished(delta: 1);
        }

        static string GetIpAddress()
        {
            var addresses = Dns.GetHostAddresses(Dns.GetHostName());
            foreach(var ip in addresses)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            throw new Exception("Ip Address not found");
        }
    }
}
