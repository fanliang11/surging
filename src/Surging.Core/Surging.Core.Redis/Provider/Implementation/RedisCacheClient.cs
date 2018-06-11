using StackExchange.Redis;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Redis.Provider.Implementation
{
    public class RedisCacheClient : ICacheClient
    { 

        public RedisCacheClient()
        {

        }
        
        public IDatabase GetClient(RedisEndpoint endpoint, int connectTimeout)
        {
            var info = endpoint as RedisEndpoint;
            Check.NotNull(info, "endpoint");
            var key = string.Format("{0}{1}{2}{3}", info.Host, info.Port, info.Password, info.DbIndex);

            var point = string.Format("{0}:{1}", info.Host, info.Port);
            var redisClient = ConnectionMultiplexer.Connect(new ConfigurationOptions()
            {
                EndPoints = { { point } },
                ServiceName = point,
                Password = info.Password,
                ConnectTimeout = connectTimeout,
                AbortOnConnectFail = false
            });
            return redisClient.GetDatabase(info.DbIndex);
        }
    }
}
