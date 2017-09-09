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
        IInjector<T> GetInjector<T>(Format<T> format);

        IExtractor<T> GetExtractor<T>(Format<T> format);
    }

    public class PropagationRegistry : IPropagationRegistry
    {
        Dictionary<Type, object> injectors = new Dictionary<Type, object>();
        Dictionary<Type, object> extractors = new Dictionary<Type, object>();

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

        public IExtractor<T> GetExtractor<T>(Format<T> format) => extractors[typeof(T)] as IExtractor<T>;

        public IInjector<T> GetInjector<T>(Format<T> format) => injectors[typeof(T)] as IInjector<T>;
    }
}
