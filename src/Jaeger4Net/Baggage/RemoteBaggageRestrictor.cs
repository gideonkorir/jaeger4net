using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jaeger4Net.Baggage
{
    public class RemoteBaggageRestrictor : IRestrictBaggage
    {
        readonly IRestrictionSource restrictionSource;
        readonly AsyncTimer timer;
        readonly RemoteRestrictorOptions options;
        int initialized;

        readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Restriction>> serviceRestrictions;

        public RemoteBaggageRestrictor(IRestrictionSource restrictionSource, RemoteRestrictorOptions options)
        {
            this.restrictionSource = restrictionSource ?? throw new ArgumentNullException(nameof(restrictionSource));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            serviceRestrictions = new ConcurrentDictionary<string, ConcurrentDictionary<string, Restriction>>();
            timer = new AsyncTimer(UpdateAsync, options.RefreshInterval, options.CancellationToken);
            timer.Start();
        }

        public Restriction Get(string service, string key)
        {
            if(initialized == 0)
            {
                return options.DenyBaggageOnInitializationFailure
                    ? Restriction.Invalid
                    : Restriction.Valid;
            }
            if (serviceRestrictions.TryGetValue(service, out var restrictions))
                if (restrictions.TryGetValue(key, out var value))
                    return value;
            return Restriction.Invalid;
        }

        async Task UpdateAsync()
        {
            if (options.Services.Length == 1)
            {
                await UpdateAsync(options.Services[0]).ConfigureAwait(false);
                return;
            }
            var tasks = new Task[options.Services.Length];
            for(int i=0; i<options.Services.Length; i++)
            {
                tasks[i] = UpdateAsync(options.Services[i]);
            }
            await Task.WhenAll(tasks);
        }

        async Task UpdateAsync(string serviceName)
        {
            try
            {
                var restrictions = await restrictionSource.FetchAsync(serviceName, options.CancellationToken)
                    .ConfigureAwait(false);
                var scopedRestrictions = serviceRestrictions.GetOrAdd(serviceName, (key) => new ConcurrentDictionary<string, Restriction>());
                if(restrictions != null)
                {
                    foreach(var restr in restrictions)
                    {
                        var value = new Restriction(true, restr.MaxValueLength);
                        scopedRestrictions.AddOrUpdate(restr.Key, value,
                            (key, old) => value
                            );
                    }
                }
                Interlocked.Exchange(ref initialized, 1); //just set initialized to 1
            }
            catch(Exception ex)
            {
                //we don't know what to do
            }
        }
    }
}
