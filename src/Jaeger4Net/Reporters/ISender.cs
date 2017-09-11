using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jaeger4Net.Reporters
{
    public interface ISender : IDisposable
    {
        int Append(Span span);

        Task<int> FlushAsync(CancellationToken cancellationToken);
    }
}
