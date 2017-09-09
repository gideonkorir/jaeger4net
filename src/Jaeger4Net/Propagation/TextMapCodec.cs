using OpenTracing.Propagation;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Jaeger4Net.Propagation
{
    public class TextMapCodec : IExtractor<ITextMap>, IInjector<ITextMap>
    {
        /**
        * Key used to store serialized span context representation
        */
        public const String SPAN_CONTEXT_KEY = "uber-trace-id";

        /**
        * Key prefix used for baggage items
        */
        public const string BAGGAGE_KEY_PREFIX = "uberctx-";

        readonly string contextKey, baggagePrefix;

        bool urlEncode;

        public TextMapCodec(bool urlEncode)
        {
            this.urlEncode = urlEncode;
            contextKey = SPAN_CONTEXT_KEY;
            baggagePrefix = BAGGAGE_KEY_PREFIX;
        }

        public TextMapCodec(bool urlEncode, string contextKey, string baggagePrefix)
        {
            this.urlEncode = urlEncode;
            this.contextKey = contextKey;
            this.baggagePrefix = baggagePrefix;
        }

        /// <summary>
        /// Extract context from the underlying text map carrier
        /// </summary>
        /// <param name="carrier"></param>
        /// <returns></returns>
        public SpanContext Extract(ITextMap carrier)
        {
            SpanContext context = null;
            Dictionary<string, string> baggage = null;
            string debugId = null;
            bool contextParsed = false;
            
            foreach(var item in carrier.GetEntries())
            {
                if(item.Key.Equals(contextKey))
                {
                    SpanContext.TryParse(Decode(item.Value), out context);
                }
                else if(item.Key.Equals(Constants.DEBUG_ID_HEADER_KEY))
                {
                    debugId = Decode(item.Value);
                }
                else if(item.Key.StartsWith(baggagePrefix))
                {
                    if (baggage == null)
                        baggage = new Dictionary<string, string>();
                    baggage.Add(item.Key.MinusPrefix(baggagePrefix), item.Value);
                }
            }

            if(!contextParsed)
            {
                if(!string.IsNullOrWhiteSpace(debugId))
                {
                    return SpanContext.Debug(debugId);
                }
                return null;
            }
            if (baggage == null)
                return context;
            return context.SetBaggage(baggage);
        }

        /// <summary>
        /// Inject the span context into the carrier.
        /// The context is serialized as: contextKey, context.ContextAsString
        /// we then add all baggage items into the carrier
        /// </summary>
        /// <param name="context"></param>
        /// <param name="carrier"></param>
        public void Inject(SpanContext context, ITextMap carrier)
        {
            carrier.Set(contextKey, Encode(context.ContextAsString));
            foreach(var entry in context.Baggage)
            {
                carrier.Set(entry.Key.WithPrefix(baggagePrefix), Encode(entry.Value));
            }
        }

        string Encode(string text) => !urlEncode ? text : WebUtility.UrlEncode(text);

        string Decode(string text) => !urlEncode ? text : WebUtility.UrlDecode(text);
    }
}
