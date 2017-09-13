using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using Jaeger4Net.Metrics;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;
using Jaeger4Net.Utils;

namespace Jaeger4Net.Reporters
{
    class WorkQueue
    {
        readonly BlockingCollection<Span> queue;
        readonly ISender sender;
        readonly ClientMetrics metrics;
        readonly WorkQueueOptions options;

        int flush;
        public Task Run { get; private set; }

        Task signalFlush;


        public WorkQueue(ISender sender, WorkQueueOptions options, ClientMetrics metrics)
        {
            this.sender = sender ?? throw new ArgumentNullException(nameof(sender));
            this.metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            queue = new BlockingCollection<Span>(options.MaxQueueSize);
        }

        public void Add(Span span)
        {
            if(!queue.TryAdd(span))
            {
                metrics.ReporterDropped(delta: 1);
            }
        }

        public void StartAsync(CancellationToken cancellation)
        {
            Run = Task.Run(async () =>
            {
                await EmptyBufferAsync(cancellation);
            }, cancellation);

            signalFlush = Task.Run(async () =>
            {
                await SignalFlushAsync(cancellation);
            }, cancellation);
        }

        async Task EmptyBufferAsync(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                if (flush > 0)
                {
                    await FlushAsync(cancellation);
                }
                AddToSender();
            }
        }

        async Task SignalFlushAsync(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                await Task.Delay(options.FlushInterval, cancellation);
                if (!cancellation.IsCancellationRequested)
                {
                    metrics.ReporterQueueLength(value: queue.Count);
                    Interlocked.Exchange(ref flush, 1);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddToSender()
        {
            if (queue.TryTake(out var span, TimeSpan.FromSeconds(1)))
                sender.Append(span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        async Task FlushAsync(CancellationToken cancellation)
        {
            try
            {
                var flushed = await sender.FlushAsync(cancellation);
                Interlocked.Exchange(ref flush, 0);
                metrics.ReporterSuccess(delta: flushed);
            }
            catch(SenderException ex)
            {
                metrics.ReporterFailure(delta: ex.DroppedSpanCount);
            }
            catch(Exception)
            {
                //log this and continue
            }
        }
    }
}
