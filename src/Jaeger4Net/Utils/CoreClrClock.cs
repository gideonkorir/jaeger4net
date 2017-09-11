using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Utils
{

    /// <summary>
    /// Core clr datetime resolution is ~ 1uc
    /// <see cref="https://github.com/dotnet/corefx/blob/aaaffdf7b8330846f6832f43700fbcc060460c9f/src/System.Diagnostics.DiagnosticSource/src/System/Diagnostics/Activity.DateTime.corefx.cs"/>
    /// <seealso cref="https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/DateTime.CoreCLR.cs"/>
    /// </summary>
    /// <returns></returns>
    public class CoreClrClock : IClock
    {
        public DateTimeOffset Now() => new DateTimeOffset(DateTime.UtcNow);
    }
}
