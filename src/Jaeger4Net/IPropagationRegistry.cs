using System;
using System.Collections.Generic;
using System.Text;
using OpenTracing;
using OpenTracing.Propagation;

namespace Jaeger4Net
{
    public interface IPropagationRegistry
    {
        ITextMap Get<T>(Format<T> format);
    }

    public class PropagationRegistry : IPropagationRegistry
    {
        Dictionary<Type, ITextMap> propagators = new Dictionary<Type, ITextMap>();

        public void Register<T>(Format<T> format, ITextMap textMap) => propagators[format.GetType()] = textMap;

        public ITextMap Get<T>(Format<T> format) => propagators[format.GetType()];
    }
}
