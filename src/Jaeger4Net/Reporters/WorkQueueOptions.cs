using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Reporters
{
    public class WorkQueueOptions
    {
        public TimeSpan FlushInterval { get; set; }
        public int MaxQueueSize { get; set; }
    }
}
