using StackExchange.Redis;
using Surging.Core.Redis.Provider.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Redis.Provider
{
    public interface ICacheClient
    {
        IDatabase GetClient(RedisEndpoint info, int connectTimeout);
        
    }
}
