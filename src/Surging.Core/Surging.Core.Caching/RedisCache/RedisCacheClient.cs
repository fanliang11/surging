using StackExchange.Redis;
using Surging.Core.Caching.HashAlgorithms;
using Surging.Core.Caching.Interfaces;
using Surging.Core.Caching.Utilities;
using Surging.Core.CPlatform.Cache;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Surging.Core.Caching.RedisCache
{
    [IdentifyCache(name: CacheTargetType.Redis)]
    public class RedisCacheClient<T> : ICacheClient<T>
        where T : class

    {
        private static readonly ConcurrentDictionary<string, Lazy<ObjectPool<T>>> _pool =
            new ConcurrentDictionary<string, Lazy<ObjectPool<T>>>();

        public RedisCacheClient()
        {

        }

        public async Task<bool> ConnectionAsync(CacheEndpoint endpoint, int connectTimeout)
        {
            ConnectionMultiplexer conn=null;
            try
            {
                var info = endpoint as ConsistentHashNode;
                var point = string.Format("{0}:{1}", info.Host, info.Port);
                  conn = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions()
                {
                    EndPoints = { { point } },
                    ServiceName = point,
                    Password = info.Password,
                    ConnectTimeout = connectTimeout
                });
                return conn.IsConnected;
            }
            catch (Exception e)
            {
                throw new CacheException(e.Message);
            }
            finally
            {
               if(conn !=null)
                conn.Close();
            }
        }

        public T GetClient(CacheEndpoint endpoint, int connectTimeout)
        {
            try
            {
                var info = endpoint as RedisEndpoint;
                Check.NotNull(info, "endpoint");
                var key = string.Format("{0}{1}{2}{3}", info.Host, info.Port, info.Password, info.DbIndex);
                if (!_pool.ContainsKey(key))
                { 
                        var objectPool = new Lazy<ObjectPool<T>>(()=>new ObjectPool<T>(() =>
                        {
                            var point = string.Format("{0}:{1}", info.Host, info.Port);
                            var redisClient = ConnectionMultiplexer.Connect(new ConfigurationOptions()
                            {
                                EndPoints = { { point } },
                                ServiceName = point,
                                Password = info.Password,
                                ConnectTimeout = connectTimeout,
                                AbortOnConnectFail = false
                            }); 
                            return redisClient.GetDatabase(info.DbIndex) as T;
                        }, info.MinSize, info.MaxSize));
                        _pool.GetOrAdd(key, objectPool);
                        return objectPool.Value.GetObject(); 
                }
                else
                {
                    return _pool[key].Value.GetObject();
                }
            }
            catch (Exception e)
            {
                throw new CacheException(e.Message);
            } 
        }
    }
}
