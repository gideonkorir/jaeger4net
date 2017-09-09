using OpenTracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jaeger4Net
{
    public class SpanContext : ISpanContext
    {
        public const byte SampledFlag = 1;
        public const byte DebugFlag = 2;

        private static IReadOnlyDictionary<string, string> Empty = new Dictionary<string, string>();
        public long TraceId { get; }
        public long SpanId { get; }
        public long ParentId { get; }
        public byte Flags { get; }
        public IReadOnlyDictionary<string, string> Baggage { get; }
        public string DebugId { get; }

        public string ContextAsString => $"{TraceId}:{SpanId}:{ParentId}:{Flags}";

        /// <summary>
        /// Retruns true when the context is only used to return the debu/correlation ID from
        /// extract() method. This happens in the situation when "jaeger-debug-id" header is 
        /// passed in the carrier to the extract() mehtod, but the request otherwise has no 
        /// span context in it. Previously this would have returned null from the extract method
        /// but now it returns a context with only the DebugId property filled in.
        /// </summary>
        public bool IsDebugIdContainerOnly => TraceId == 0 && !string.IsNullOrWhiteSpace(DebugId);

        public bool IsSampled => (Flags & SampledFlag) == SampledFlag;
        public bool IsDebug => (Flags & DebugFlag) == DebugFlag;

        public SpanContext(long traceId, long spanId, long parentId, byte flags)
           : this(traceId, spanId, parentId, flags, Empty, null)
        {

        }

        public SpanContext(long traceId, long spanId, long parentId, byte flags,
            IReadOnlyDictionary<string, string> baggage, string debugId)
        {
            TraceId = traceId;
            SpanId = spanId;
            ParentId = parentId;
            Flags = flags;
            Baggage = baggage ?? throw new ArgumentNullException(nameof(baggage));
            DebugId = debugId;
        }
        public IEnumerable<KeyValuePair<string, string>> GetBaggageItems() => Baggage;

        public SpanContext AppendBaggageItem(string key, string value)
        {
            var bag = new Dictionary<string, string>(Baggage.Count + 1)
            {
                {  key, value }
            };
            foreach (var item in Baggage)
                bag.Add(item.Key, item.Value);


            return new SpanContext(TraceId,
                SpanId,
                ParentId,
                Flags,
                bag,
                DebugId
                );
        }

        public SpanContext SetBaggage(IReadOnlyDictionary<string, string> newBaggage)
            => new SpanContext(TraceId, SpanId, ParentId, Flags, newBaggage, DebugId);

        public SpanContext SetFlags(byte flags)
            => new SpanContext(TraceId, SpanId, ParentId, flags, Baggage, DebugId);

        public override string ToString() => ContextAsString;


        /// <summary>
        /// Returns a debug span context with DebugId set to the supplied string. This is
        /// used when "jaeger-debug-id" header is present in the request headers and forces
        /// the trace to be sampled as debug trace, and the value of header recorded as a span
        /// tag to serve as a searchable correlation ID.
        /// </summary>
        /// <param name="debugId">Id of the debug session</param>
        /// <returns></returns>
        public static SpanContext Debug(string debugId)
            => new SpanContext(0, 0, 0, 0, Empty, debugId);

        /// <summary>
        /// Check <see cref="TryParse(string, out SpanContext)"/> for how this works, it just throws
        /// if the parse was unsuccessful
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SpanContext Parse(string value)
        {
            if(!TryParse(value, out var context))
            {
                throw new InvalidOperationException($"Could not parse the value {value} to a span context");
            }
            return context;
        }

        /// <summary>
        /// Converts a string to a span context.
        /// The string should be 4 parts separated by a colon (:)
        /// e.g. 1:2:3:4
        /// the parts[0:2] are parsed as long and form the traceid, parentid,spanid
        /// part[3] is parsed as byte and passed to flags
        /// </summary>
        /// <param name="value">The string to parse</param>
        /// <param name="context">Non-null context if the parse was successful otherwise false</param>
        /// <returns>true if the span context was parsed</returns>
        public static bool TryParse(string value, out SpanContext context)
        {
            context = null;
            if (string.IsNullOrWhiteSpace(value))
                return false;
            var parts = value.Split(':');
            if (parts.Length != 4)
                return false;
            context = new SpanContext(
                long.Parse(parts[0]),
                long.Parse(parts[1]),
                long.Parse(parts[2]),
                byte.Parse(parts[3])
                );
            return true;
        }
    }
}
