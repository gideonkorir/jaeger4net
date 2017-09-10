using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jaeger4Net.Baggage
{
    class AsyncTimer
    {
        readonly TimeSpan interval;
        readonly Func<Task> action;
        readonly CancellationToken cancellationToken;

        Task task;

        public AsyncTimer(Func<Task> action, TimeSpan interval, CancellationToken cancellationToken)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
            this.interval = interval;
            this.cancellationToken = cancellationToken;
        }

        public void Start()
        {
            if (task != null)
                return;
            task = Task.Run(async () =>
            {
                while(!cancellationToken.IsCancellationRequested)
                {
                    await action().ConfigureAwait(false);
                    await Task.Delay(interval, cancellationToken).ConfigureAwait(false); 
                }
            }, cancellationToken);
        }
    }
}
