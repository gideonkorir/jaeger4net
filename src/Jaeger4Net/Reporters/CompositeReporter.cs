using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Reporters
{
    public class CompositeReporter : IReporter
    {
        readonly List<IReporter> reporters;

        public CompositeReporter(params IReporter[] reporters)
        {
            this.reporters = new List<IReporter>(reporters);
        }

        public void Report(Span span)
        {
            foreach (var r in reporters)
                r.Report(span);
        }

        public void Dispose()
            => reporters.ForEach(c => c.Dispose());
    }
}
