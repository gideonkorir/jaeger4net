using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Utils
{
    public interface IClock
    {
        DateTimeOffset Now();
    }
}
