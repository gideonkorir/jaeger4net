using OpenTracing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using OTags = OpenTracing.Tags;

namespace Jaeger4Net
{
    public class Span : ISpan
    {
        static readonly IReadOnlyList<Reference> NoRef = new List<Reference>();
        static readonly IReadOnlyList<LogData> NoLog = new List<LogData>();

        string operationName;
        readonly Tracer tracer;
        SpanContext context;
        readonly Dictionary<string, object> tags = new Dictionary<string, object>();
        List<LogData> logs = null;
        List<Reference> references = null;
        bool finishCalled = false, disposed = false;

        public DateTimeOffset Start { get; }

        public DateTimeOffset? End { get; private set; }

        public long Duration => End.Value.Ticks - Start.Ticks;
        public Tracer Tracer => tracer;

        public IReadOnlyList<Reference> References => references == null ? NoRef : references;

        public IReadOnlyDictionary<string, object> Tags
        {
            get { lock (this) return tags; }
        }

        public string OperationName
        {
            get { lock (this) return operationName; }
        }

        public string ServiceName
        {
            get { lock (this) return tracer.ServiceName; }
        }

        public ISpanContext Context
        {
            get { lock (this) return context; }
        }

        public IReadOnlyList<LogData> Logs
        {
            get { lock (this) return logs ?? NoLog; }
        }

        public Span(Tracer tracer, string operationName,
            SpanContext context,
            DateTimeOffset start,
            IDictionary<string, object> tags,
            List<Reference> references)
        {
            this.tracer = tracer;
            this.operationName = operationName;
            this.context = context;
            this.Start = start;
            this.references = references;
            foreach(var item in tags)
            {
                SetTagObject(item.Key, item.Value);
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                if (finishCalled)
                    return;
                if (!disposed)
                {
                    Finish();
                    disposed = true;
                }
            }
        }

        public void Finish() => Finish(tracer.Clock.Now());

        public void Finish(DateTimeOffset finishTimestamp)
        {
            lock(this)
            {
                finishCalled = true;
                End = finishTimestamp;
            }
            if(context.IsSampled)
                tracer.Report(this);
        }

        public string GetBaggageItem(string key)
        {
            lock (this)
                return context.Baggage[key];
        }

        

        /// <summary>
        /// Creates a new Span with baggage set iff both key && value != null 
        /// otherwise returns the same span with noop
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ISpan SetBaggageItem(string key, string value)
        {
            if (ReferenceEquals(key, null) || ReferenceEquals(value, null))
                return this;
            throw new NotImplementedException();
        }

        public ISpan SetOperationName(string operationName)
        {
            lock(this)
            {
                this.operationName = operationName;
            }
            return this;
        }

        #region logs

        public ISpan Log(IEnumerable<KeyValuePair<string, object>> fields)
            => Log(tracer.Clock.Now(), fields);

        public ISpan Log(DateTimeOffset timestamp, IEnumerable<KeyValuePair<string, object>> fields)
        {
            lock(this)
            {
                if (fields == null)
                    return this;
            }
            if(context.IsSampled)
            {
                if (logs == null)
                {
                    logs = new List<LogData>();
                }
                logs.Add(new LogData(timestamp, fields));
            }
            return this;
        }
        

        public ISpan Log(string eventName) => Log(tracer.Clock.Now(), eventName);

        public ISpan Log(DateTimeOffset timestamp, string eventName)
        {
            lock (this)
            {
                if (string.IsNullOrWhiteSpace(eventName))
                    return this;
            }
            if (context.IsSampled)
            {
                if (logs == null)
                {
                    logs = new List<LogData>();
                }
                logs.Add(new LogData(timestamp, eventName, payload: null));
            }
            return this;
        }

#endregion logs

        #region tags
        public ISpan SetTag(string key, bool value) => SetTagObject(key, value);

        public ISpan SetTag(string key, double value) => SetTagObject(key, value);

        public ISpan SetTag(string key, int value) => SetTagObject(key, value);

        public ISpan SetTag(string key, string value) => SetTagObject(key, value);

        Span SetTagObject(string key, object value)
        {
            if(string.Equals(key, OTags.SamplingPriority) && value is int priority)
            {
                byte newFlags =
                    priority > 0 ? (byte)(context.Flags | SpanContext.SampledFlag | SpanContext.DebugFlag)
                    : (byte)(context.Flags & ~SpanContext.SampledFlag);
                context = context.SetFlags(newFlags);
            }
            if (context.IsSampled)
                tags.Add(key, value);
            return this;
        }

#endregion tags

        public override string ToString() =>
            $"{context.ContextAsString} - {operationName}";



        readonly static AsyncLocal<Span> current = new AsyncLocal<Span>();

        public static Span Current
        {
            get => current.Value;
            //only settable by the library.
            //similar to ActiveSpanSource in java
            internal set => current.Value = value;
        }
    }
}
