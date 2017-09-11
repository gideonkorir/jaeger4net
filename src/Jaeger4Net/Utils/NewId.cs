using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Utils
{
    static class NewId
    {
        public static Func<long> Next { get; set; } = new ThreadLocalRandom().Next;
    }
}
