using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net
{
    /// <summary>
    /// Logger factory to be used by the client library
    /// </summary>
    public static class Log
    {
        public static ILoggerFactory LogFactory { get; set; }
            = new LoggerFactory();

        public static ILogger<T> Create<T>()
            => LogFactory.CreateLogger<T>();
    }
}
