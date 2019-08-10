using System.Collections.Generic;

namespace Surging.Apm.Skywalking.Abstractions.Tracing
{
    public interface ICarrierHeaderCollection : IEnumerable<KeyValuePair<string, string>>
    {
        void Add(string key, string value);
    }
}