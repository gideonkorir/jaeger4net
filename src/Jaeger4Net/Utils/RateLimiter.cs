using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Utils
{
    public class RateLimiter
    {
        readonly double creditsPerTick;
        double maxBalance, balance;
        private readonly IClock clock;
        long previousTicksValue;

        public RateLimiter(double creditsPerSecond, double maxBalance, IClock clock)
        {
            this.clock = clock;
            this.maxBalance = maxBalance;
            creditsPerTick = creditsPerSecond / TimeSpan.TicksPerSecond;
            balance = maxBalance;
        }

        public bool CheckCredit(double itemCost)
        {
            var currentTicks = clock.Now().Ticks;
            long elapsed = currentTicks - previousTicksValue;
            previousTicksValue = currentTicks;
            balance += elapsed * creditsPerTick;
            //balance shouldn't be greater than max balance
            balance = balance > maxBalance ? maxBalance : balance;
            if(balance >= itemCost)
            {
                balance -= itemCost;
                return true;
            }
            return false;
        }


    }
}
