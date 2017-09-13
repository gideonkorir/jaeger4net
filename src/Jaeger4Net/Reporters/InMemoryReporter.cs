using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Reporters
{
    class InMemoryReporter : IReporter
    {
        readonly List<Span> spans;

        public IReadOnlyList<Span> Spans => spans;

        public InMemoryReporter()
        {
            spans = new List<Span>();
        }

        public void Report(Span span)
        {
            lock(spans)
                spans.Add(span);
        }
        public void Dispose()
        {
            lock (spans)
                spans.Clear();
        }
    }
}
