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

        public int Report(Span span)
        {
            var rem = 0;
            foreach (var r in reporters)
                rem = r.Report(span);
            return rem;
        }

        public void Dispose()
            => reporters.ForEach(c => c.Dispose());
    }
}
