using ServiceStack.Redis;
using Surging.Core.Caching.Interfaces;
using Surging.Core.Caching.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching.RedisCache
{
    [IdentifyCache(name: CacheTargetType.Redis)]
    public class RedisCacheClient : ICacheClient<IRedisClient>
    {
        private static readonly ConcurrentDictionary<string, ObjectPool<IRedisClient>> _pool =
            new ConcurrentDictionary<string, ObjectPool<IRedisClient>>();

        public IRedisClient GetClient(CacheEndpoint endpoint, int connectTimeout)
        {
            try
            {
                var info = endpoint as RedisEndpoint;
                Check.NotNull(info, "endpoint");
                var key = string.Format("{0}{1}{2}{3}", info.Host, info.Port, info.Password, info.DbIndex);
                if (!_pool.ContainsKey(key))
                {
                    var objectPool = new ObjectPool<IRedisClient>(() =>
                    {
                        var redisClient = new RedisClient(info.Host, info.Port, info.Password, info.DbIndex);
                        redisClient.ConnectTimeout = connectTimeout;
                        return redisClient;
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
