using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Reporters
{
    class NoopReporter : IReporter
    {
        public void Dispose()
        {
        }

        public int Report(Span span)
            => 0;
    }
}
