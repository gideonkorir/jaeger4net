using Jaeger4Net.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Jaeger4Net.Tests.Utils
{
    public class RateLimiterTests
    {
        [Fact]
        public void Test_RateLimiter_Whole_Numbers()
        {
            var clock = new MockClock(value: DateTimeOffset.UtcNow);
            RateLimiter limiter = new RateLimiter(2, 2, clock);

            //move time forward by 1ms we have initial reserve
            //of 2 but that's max we should get
            clock.AddAndSet(TimeSpan.FromMilliseconds(1));
            Assert.True(limiter.CheckCredit(1.0));
            Assert.True(limiter.CheckCredit(1.0));
            Assert.False(limiter.CheckCredit(1.0));

            //move time by 250ms still not enought credit for 1 item
            clock.AddAndSet(TimeSpan.FromMilliseconds(250));
            Assert.False(limiter.CheckCredit(1.0));

            //move time 500ms forward, enough to pay for 1 item
            clock.AddAndSet(TimeSpan.FromMilliseconds(500));
            Assert.True(limiter.CheckCredit(1.0));
            Assert.False(limiter.CheckCredit(1.0));

            //move time 5sec forwad, enough for 10 but should be capped at 2
            clock.AddAndSet(TimeSpan.FromSeconds(5));
            Assert.True(limiter.CheckCredit(1.0));
            Assert.True(limiter.CheckCredit(1.0));
            Assert.False(limiter.CheckCredit(1.0));
            Assert.False(limiter.CheckCredit(1.0));
        }

        [Fact]
        public void Test_RateLimiter_LessThan_One()
        {
            var clock = new MockClock(DateTime.UtcNow);
            var limiter = new RateLimiter(0.5, 0.5, clock);

            clock.AddAndSet(TimeSpan.FromMilliseconds(1));
            Assert.True(limiter.CheckCredit(0.25));
            Assert.True(limiter.CheckCredit(0.25));
            Assert.False(limiter.CheckCredit(0.25));

            //move forward 250ms, not enough for 0.25 item
            clock.AddAndSet(TimeSpan.FromMilliseconds(250));
            Assert.False(limiter.CheckCredit(0.25));

            //move format 500ms enough to pay for 0.25 item
            clock.AddAndSet(TimeSpan.FromMilliseconds(500));
            Assert.True(limiter.CheckCredit(0.25));
            Assert.False(limiter.CheckCredit(0.25));

            //move forward 5sec enough for 20 items but only 0.5 allowed
            clock.AddAndSet(TimeSpan.FromSeconds(5));
            Assert.True(limiter.CheckCredit(0.5));
            Assert.False(limiter.CheckCredit(0.1));
        }

        [Fact]
        public void Test_MaxBalance()
        {
            var clock = new MockClock();
            var limiter = new RateLimiter(0.1, 1, clock);

            clock.AddAndSet(TimeSpan.FromMilliseconds(0.1));
            Assert.True(limiter.CheckCredit(1.0));
            Assert.False(limiter.CheckCredit(0.1));

            //move 20 seconds format enough for 2 items but should fix to 1
            clock.AddAndSet(TimeSpan.FromSeconds(20));
            Assert.True(limiter.CheckCredit(1.0));
            Assert.False(limiter.CheckCredit(0.1));
        }
    }

    public class MockClock : IClock
    {
        public DateTimeOffset Value { get; set; }

        public MockClock()
        {

        }

        public MockClock(DateTimeOffset value)
        {
            Value = value;
        }

        public void AddAndSet(TimeSpan span)
        {
            Value = Value.AddTicks(span.Ticks);
        }

        public DateTimeOffset Now() => Value;
    }
}
