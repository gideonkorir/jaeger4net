using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jaeger4Net.Reporters
{
    /// <summary>
    /// Reports finished spns to something that collects those spans.
    /// Default implementation is remote reporter that sends spans out of process
    /// </summary>
    public interface IReporter : IDisposable
    {
        int Report(Span span);
    }
}
