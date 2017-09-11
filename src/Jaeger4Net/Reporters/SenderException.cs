using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Reporters
{
    /// <summary>
    /// Exceptions raised when trying to send a span
    /// </summary>
    public class SenderException : Exception
    {
        public int DroppedSpanCount { get; }

        public SenderException(string message, int droppedSpanCount)
            : base(message)
        {
            DroppedSpanCount = droppedSpanCount;
        }

        public SenderException(string message, Exception ex, int droppedSpanCount)
            : base(message, ex)
        {
            DroppedSpanCount = droppedSpanCount;
        }
    }
}
