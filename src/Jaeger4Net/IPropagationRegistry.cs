using System;
using System.Collections.Generic;
using System.Text;
using OpenTracing;
using OpenTracing.Propagation;
using Jaeger4Net.Propagation;

namespace Jaeger4Net
{
    public interface IPropagationRegistry
    {
        bool TryGetInjector<T>(Format<T> format, out IInjector<T> injector);

        bool TryGetExtractor<T>(Format<T> format, out IExtractor<T> extractor);
    }

    public class PropagationRegistry : IPropagationRegistry
    {
        readonly Dictionary<Type, object> injectors = new Dictionary<Type, object>();
        readonly Dictionary<Type, object> extractors = new Dictionary<Type, object>();

        public void Register<T>(IInjector<T> injector)
        {
            if (injector == null)
                throw new ArgumentNullException(nameof(injector));
            injectors.Add(typeof(T), injector);
        }

        public void Register<T>(IExtractor<T> extractor)
        {
            if (extractor == null)
                throw new ArgumentNullException(nameof(extractor));
            extractors.Add(typeof(T), extractor);
        }

        public bool TryGetExtractor<T>(Format<T> format, out IExtractor<T> extractor)
        {
            if(extractors.TryGetValue(typeof(T), out var obj) && obj is IExtractor<T> temp)
            {
                extractor = temp;
                return true;
            }
            extractor = null;
            return false;
        }

        public bool TryGetInjector<T>(Format<T> format, out IInjector<T> injector)
        {
            if(injectors.TryGetValue(typeof(T), out var obj) && obj is IInjector<T> temp)
            {
                injector = temp;
                return true;
            }
            injector = null;
            return false;
        }
    }
}
