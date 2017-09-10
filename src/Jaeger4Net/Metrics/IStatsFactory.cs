using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Metrics
{
    /// <summary>
    /// Creates specific metrics
    /// </summary>
    public interface IStatsFactory
    {
        //Returns a counter bound to the specified name and
        //that with tags to include
        Counter Counter(string name, params Tag[] tags);


        //Returns a timer bound to the specified metric name
        Timer Timer(string name, params Tag[] tags);

        //Returns a gauge bound to the specified metric name
        Gauge Gauge(string name, params Tag[] tags);
    }
}
