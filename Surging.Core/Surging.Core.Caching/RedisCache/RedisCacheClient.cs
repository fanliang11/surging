using StackExchange.Redis;
using Surging.Core.Caching.Interfaces;
using Surging.Core.Caching.Utilities;
using System;
using System.Collections.Concurrent;

namespace Surging.Core.Caching.RedisCache
{
    [IdentifyCache(name: CacheTargetType.Redis)]
    public class RedisCacheClient : ICacheClient<IDatabase>
    {
        private static readonly ConcurrentDictionary<string, ObjectPool<IDatabase>> _pool =
            new ConcurrentDictionary<string, ObjectPool<IDatabase>>();

        public IDatabase GetClient(CacheEndpoint endpoint, int connectTimeout)
        {
            try
            {
                var info = endpoint as RedisEndpoint;
                Check.NotNull(info, "endpoint");
                var key = string.Format("{0}{1}{2}{3}", info.Host, info.Port, info.Password, info.DbIndex);
                if (!_pool.ContainsKey(key))
                {
                    var objectPool = new ObjectPool<IDatabase>(() =>
                    {
                        var point = string.Format("{0}:{1}", info.Host, info.Port);
                        var redisClient = ConnectionMultiplexer.Connect(new ConfigurationOptions()
                        {
                            EndPoints = { { point } },
                            ServiceName = point,
                            Password = info.Password,
                            ConnectTimeout = connectTimeout
                        });
                        return redisClient.GetDatabase(info.DbIndex);
                    }, info.MinSize, info.MaxSize);
                    _pool.GetOrAdd(key, objectPool);
                    return objectPool.GetObject();
                }
                else
                {
                    return _pool[key].GetObject();
                }
            }
            catch (Exception e)
            {
                throw new CacheException(e.Message);
            }
        }
    }
}