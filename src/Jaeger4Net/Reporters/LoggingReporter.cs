using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Reporters
{
    public class LoggingReporter : IReporter
    {
        readonly ILogger<LoggingReporter> logger;

        public LoggingReporter(ILogger<LoggingReporter> logger)
        {
            this.logger = logger;
        }
        public void Report(Span span)
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Span reported: {span}", span);
        }
        public void Dispose()
        {
        }
    }
}
