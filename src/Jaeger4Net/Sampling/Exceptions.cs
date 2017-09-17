using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Sampling
{
    public class SamplingStrategyRetrieveException : Exception
    {
       public SamplingStrategyRetrieveException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }

    public class ParseException : Exception
    {
        public string Input { get; }

        public ParseException(string message, Exception exception, string input)
            : base(message, exception)
        {
            Input = input;
        }
    }
}
