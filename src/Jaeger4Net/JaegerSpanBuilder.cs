using OpenTracing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Jaeger
{
    class JaegerSpanBuilder : ISpanBuilder
    {
        List<Reference> references = null;
        Dictionary<string, object> tags = new Dictionary<string, object>();
        DateTimeOffset start;
        string operationName;
        Tracer tracer;
        long parentId = 0;

        public JaegerSpanBuilder(Tracer tracer, string operationName)
        {
            this.tracer = tracer;
            this.operationName = operationName;
        }

        public ISpanBuilder AddReference(string referenceType, ISpanContext referencedContext)
        {
            if(referencedContext is SpanContext context)
            {
                if (references == null)
                    references = new List<Reference>(1); //most of the time we only have a single item
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

        public ISpan Start()
        {
            throw new NotImplementedException();
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
    }
}
