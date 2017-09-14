using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Jaeger4Net.Utils
{
    /// <summary>
    /// More info at 
    /// https://stackoverflow.com/questions/19270507/correct-way-to-use-random-in-multithread-application
    /// </summary>
    public class ThreadLocalRandom
    {
        static int ticks = Environment.TickCount;

        /// <summary>
        /// Each thread has it's own random generator. The random seed
        /// should hopefully allow for more spread of ids
        /// </summary>
        static readonly ThreadLocal<Random> random =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref ticks)));

        public long Next()
        {
            var bytes = new byte[8];
            random.Value.NextBytes(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }
    }
}
