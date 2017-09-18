using Jaeger4Net.Utils;
using OpenTracing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net
{
    class SpanBuilder : ISpanBuilder
    {
        readonly List<Reference> references = new List<Reference>(1); //most of the time we only have a single item
        readonly Dictionary<string, object> tags = new Dictionary<string, object>();
        DateTimeOffset start;
        readonly string operationName;
        readonly Tracer tracer;

        public SpanBuilder(Tracer tracer, string operationName)
        {
            this.tracer = tracer;
            this.operationName = operationName;
        }

        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            if(referencedContext is SpanContext context)
            {                
                if (!string.Equals(References.ChildOf, referenceType)
                    && !string.Equals(References.FollowsFrom, referenceType))
                    return this;
                references.Add(new Reference(context, referenceType));
            }
            return this;
        }

        public ISpanBuilder AsChildOf(ISpan parent)
            => AddReference(References.ChildOf, parent.Context);

        public ISpanBuilder AsChildOf(ISpanContext parent)
            => AddReference(References.ChildOf, parent);

        public ISpanBuilder FollowsFrom(ISpan parent)
            => AddReference(References.FollowsFrom, parent.Context);

        public ISpanBuilder FollowsFrom(ISpanContext parent)
            => AddReference(References.FollowsFrom, parent);

        /// <summary>
        /// Starts a new span also setting Span.Current
        /// </summary>
        /// <returns></returns>
        public ISpan Start()
        {
            if (start == default(DateTimeOffset))
                start = tracer.Clock.Now(); //set the start date if necessary

            if (references.Count == 0 && Span.Current != null)
                AsChildOf(Span.Current);

            var context = GetStartContext();

            var span = new Span(
                tracer,
                operationName,
                context,
                start,
                tags,
                references
                );
            Span.Current = span;
            return span;
        }

        /// <summary>
        /// Get the appropriate context to start the span with
        /// </summary>
        /// <returns></returns>
        SpanContext GetStartContext()
        {
            if (references.Count == 0)
                return CreateNewContext(); //if we have no propagating context we start a new context
            else if (DebugId != null)
                return CreateNewContext(DebugId); //if we are debugging create a new context with DebugId set
            else
                return CreateChildContext(); //we aren't debugging & we have some propagating context
        }

        public ISpanBuilder WithStartTimestamp(DateTimeOffset startTimestamp)
        {
            start = startTimestamp;
            return this;
        }

        public ISpanBuilder WithTag(string key, bool value)
        {
            tags.Add(key, value);
            return this;
        }

        public ISpanBuilder WithTag(string key, double value)
        {
            tags.Add(key, value);
            return this;
        }

        public ISpanBuilder WithTag(string key, int value)
        {
            tags.Add(key, value);
            return this;
        }

        public ISpanBuilder WithTag(string key, string value)
        {
            tags.Add(key, value);
            return this;
        }

        /// <summary>
        /// Baggage items propagate accross processes. If this is a parent
        /// span then we will have no references (ChildOf | FollowsFrom)
        /// and as such this method will return null otherwise we return a
        /// map contain all baggage items for all references
        /// </summary>
        /// <returns></returns>
        IReadOnlyDictionary<string, string> CreateChildBaggage()
        {
            Dictionary<string, string> baggage = null;
            if (references.Count == 1)
                return references[0].Context.Baggage;
            foreach(var reference in references)
            {
                if(reference.Context.Baggage != null)
                {
                    if (baggage == null)
                        baggage = new Dictionary<string, string>();
                    baggage.AddRange(reference.Context.Baggage);
                }
            }
            return baggage;
        }

        /// <summary>
        /// Returns a child context. Called when we have
        /// references
        /// </summary>
        /// <returns></returns>
        SpanContext CreateChildContext()
        {
            var preferredReference = GetPreferredReference();
            if(IsRpcServer)
            {
                if(IsSampled)
                {
                    tracer.Metrics.TracesJoinedSampled(delta: 1);
                }
                else
                {
                    tracer.Metrics.TracesJoinedNotSampled(delta: 1);
                }
            }

            return new SpanContext(
                preferredReference.TraceId,
                NewId.Next(),
                preferredReference.ParentId,
                preferredReference.Flags,
                CreateChildBaggage(),
                null
                );
        }

        //will only be invoked when we have references
        SpanContext GetPreferredReference()
        {
            if (references.Count == 1)
                return references[0].Context;
            var preferredRef = references[0];

            //child of takes precedence as a preferred parent: Tacer.java line:350
            foreach (var reference in references)
            {
                //we are a child of another span
                if (string.Equals(References.ChildOf, reference.Type))
                {
                    //if the 1st item we took wasn't a child of relationship
                    //we replace it
                    if (!string.Equals(References.ChildOf, preferredRef.Type))
                    {
                        preferredRef = reference;
                        break;
                    }
                }
            }
            return preferredRef.Context;

        }

        SpanContext CreateNewContext(string debugId = null)
        {
            var id = NewId.Next();
            byte flags = 0;
            if(debugId != null)
            {
                flags |= SpanContext.SampledFlag | SpanContext.DebugFlag;
                tags.Add(Constants.DEBUG_ID_HEADER_KEY, debugId);
                tracer.Metrics.TraceStartedSampled(delta: 1);
            }
            else
            {
                var samplingStatus = tracer.Sampler.Sample(operationName, id);
                if(samplingStatus)
                {
                    flags |= SpanContext.SampledFlag;
                    tags.AddRange(samplingStatus.Tags);
                    tracer.Metrics.TraceStartedSampled(delta: 1);
                }
                else
                {
                    tracer.Metrics.TraceStartedNotSampled(delta: 1);
                }
            }
            return new SpanContext(id, id, 0, flags);
        }

        /// <summary>
        /// Check if this span should be sampled. If any of the references we have
        /// is sampled then we are also sampled.
        /// </summary>
        bool IsSampled => references.Exists(c => c.Context.IsSampled);

        /// <summary>
        /// Check if the span is part of a debug trace. 
        /// </summary>
        string DebugId
        {
            get
            {
                if (references.Count == 1 && references[0].Context.IsDebugIdContainerOnly)
                    return references[0].Context.DebugId;
                return null;
            }
        }

        /// <summary>
        /// Checks to see we are a server span i.e. we are handling a client request.
        /// Returns true if tags["span.kind"] == "server"
        /// </summary>
        internal bool IsRpcServer
        {
            get
            {
                if (tags.TryGetValue(Tags.SpanKind, out var kindObj) && kindObj is string kind)
                    return Tags.SpanKindServer.Equals(kind);
                return false;
            }
        }
    }
}
