using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jaeger4Net.Baggage
{
    /// <summary>
    /// Response from remote server
    /// </summary>
    public struct RestrictionResponse
    {
        public string Key { get; }
        public int MaxValueLength { get; }

        public RestrictionResponse(string key, int maxValueLength)
        {
            Key = key;
            MaxValueLength = maxValueLength;
        }

        public static implicit operator Restriction(RestrictionResponse response)
            => new Restriction(true, response.MaxValueLength);
    }

    /// <summary>
    /// Fetches restrictions from a remote source
    /// </summary>
    public interface IRestrictionSource
    {
        Task<IList<RestrictionResponse>> FetchAsync(string serviceName, CancellationToken cancellationToken);
    }
}
