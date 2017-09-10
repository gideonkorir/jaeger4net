using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jaeger4Net.Baggage
{
    public interface IResponseDeserializer
    {
        RestrictionResponse[] Parse(string response);
    }

    public class JsonResponseSerializer : IResponseDeserializer
    {
        public RestrictionResponse[] Parse(string response)
        {
            return JsonConvert.DeserializeObject<RestrictionResponse[]>(response);
        }
    }
}
