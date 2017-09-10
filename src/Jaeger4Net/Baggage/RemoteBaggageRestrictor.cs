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

        readonly ConcurrentDictionary<string, Restriction> serviceRestrictions;

        public RemoteBaggageRestrictor(IRestrictionSource restrictionSource, RemoteRestrictorOptions options)
        {
            this.restrictionSource = restrictionSource ?? throw new ArgumentNullException(nameof(restrictionSource));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            serviceRestrictions = new ConcurrentDictionary<string, Restriction>();
            timer = new AsyncTimer(UpdateAsync, options.RefreshInterval, options.CancellationToken);
        }

        public Restriction Get(string service, string key)
        {
            if(initialized == 0)
            {
                return options.DenyBaggageOnInitializationFailure
                    ? Restriction.Invalid
                    : Restriction.Valid;
            }
            if (serviceRestrictions.TryGetValue(key, out var value))
                return value;
            return Restriction.Invalid;
        }

        public async Task UpdateAsync()
        {
            try
            {
                var restrictions = await restrictionSource.FetchAsync(options.ServiceName, options.CancellationToken)
                    .ConfigureAwait(false);
                if(restrictions != null)
                {
                    foreach(var restr in restrictions)
                    {
                        var value = new Restriction(true, restr.MaxValueLength);
                        serviceRestrictions.AddOrUpdate(restr.Key, value,
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
