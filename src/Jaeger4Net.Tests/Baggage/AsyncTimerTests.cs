using Jaeger4Net.Baggage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jaeger4Net.Tests.Baggage
{
    public class AsyncTimerTests
    {
        [Fact]
        public async Task AsyncTimer_Invokes_Action_On_Start()
        {
            int count = 0;
            var timer = new AsyncTimer(() =>
            {
                count += 1;
                return Task.CompletedTask;
            }, Timeout.InfiniteTimeSpan, default(CancellationToken));
            timer.Start();
            await Task.Delay(TimeSpan.FromSeconds(1));
            Assert.Equal(1, count);
        }
    }
}
