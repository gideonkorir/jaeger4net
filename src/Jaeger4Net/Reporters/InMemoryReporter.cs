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

        public int Report(Span span)
        {
            lock (spans)
            {
                spans.Add(span);
                return spans.Count;
            }

        }
        public void Dispose()
        {
            lock (spans)
                spans.Clear();
        }
    }
}
