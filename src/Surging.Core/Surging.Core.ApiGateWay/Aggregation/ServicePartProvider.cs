using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ApiGateWay.Aggregation
{
    public class ServicePartProvider : IServicePartProvider
    {
        public bool IsPart(string routhPath)
        {
            return true;
        }

        public   Task<T> Merge<T>(string routhPath, Dictionary<string, object> param)
        {
            return Task.FromResult(default(T));
        }
    }
}
