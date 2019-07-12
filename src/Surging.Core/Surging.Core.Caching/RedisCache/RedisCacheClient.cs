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
    /// <summary>
    /// Defines the <see cref="RedisCacheClient{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [IdentifyCache(name: CacheTargetType.Redis)]
    public class RedisCacheClient<T> : ICacheClient<T>
        where T : class
    {
        #region 字段

        /// <summary>
        /// Defines the _pool
        /// </summary>
        private static readonly ConcurrentDictionary<string, Lazy<ObjectPool<T>>> _pool =
            new ConcurrentDictionary<string, Lazy<ObjectPool<T>>>();

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCacheClient{T}"/> class.
        /// </summary>
        public RedisCacheClient()
        {
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The ConnectionAsync
        /// </summary>
        /// <param name="endpoint">The endpoint<see cref="CacheEndpoint"/></param>
        /// <param name="connectTimeout">The connectTimeout<see cref="int"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        public async Task<bool> ConnectionAsync(CacheEndpoint endpoint, int connectTimeout)
        {
            ConnectionMultiplexer conn = null;
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
                if (conn != null)
                    conn.Close();
            }
        }

        /// <summary>
        /// The GetClient
        /// </summary>
        /// <param name="endpoint">The endpoint<see cref="CacheEndpoint"/></param>
        /// <param name="connectTimeout">The connectTimeout<see cref="int"/></param>
        /// <returns>The <see cref="T"/></returns>
        public T GetClient(CacheEndpoint endpoint, int connectTimeout)
        {
            try
            {
                var info = endpoint as RedisEndpoint;
                Check.NotNull(info, "endpoint");
                var key = string.Format("{0}{1}{2}{3}", info.Host, info.Port, info.Password, info.DbIndex);
                if (!_pool.ContainsKey(key))
                {
                    var objectPool = new Lazy<ObjectPool<T>>(() => new ObjectPool<T>(() =>
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

        #endregion 方法
    }
}