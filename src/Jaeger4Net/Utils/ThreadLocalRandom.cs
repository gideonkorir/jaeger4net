using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Jaeger4Net.Utils
{
    public class ThreadLocalRandom
    {
        static int ticks = Environment.TickCount;

        /// <summary>
        /// Each thread has it's own random generator. The random seed
        /// should hopefully allow for more spread of ids
        /// </summary>
        static readonly ThreadLocal<Random> random =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref ticks)));

        /// <summary>
        /// Bytes which are filled in by the local random.
        /// </summary>
        static readonly ThreadLocal<byte[]> bytes =
            new ThreadLocal<byte[]>(() => new byte[8]);

        public long Next()
        {            
            random.Value.NextBytes(bytes.Value);
            return BitConverter.ToInt64(bytes.Value, 0);
        }
    }
}
