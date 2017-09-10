using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Metrics
{
    //Represents a counter metric. Delta increases
    //or decreases the counter value
    public delegate void Counter(long delta);

    //REpresents a gauge metric. Value sets the metric value
    public delegate void Gauge(long value);

    //Represents a timer metric. Duration is how long operation took
    public delegate void Timer(TimeSpan duration);
}
