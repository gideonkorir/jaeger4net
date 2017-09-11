using Jaeger4Net.Metrics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jaeger4Net.Reporters
{
    public class RemoteReporter : IReporter
    {
        public const int DEFAULT_CLOSE_ENQUEUE_TIMEOUT_MILLIS = 1000;

        readonly ClientMetrics metrics;
        readonly WorkQueue workQueue;

        public RemoteReporter(ISender sender, WorkQueueOptions options, ClientMetrics metrics)
        {
            this.metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
            workQueue = new WorkQueue(sender, options, metrics);            
        }

        public void Start(CancellationToken cancellation)
            => workQueue.StartAsync(cancellation);

        public int Report(Span span)
            => workQueue.Add(span);

        public void Dispose()
        {
            //nothing to dispose
        }
    }
}
