using StackExchange.Redis;
using Surging.Core.Caching.AddressResolvers;
using Surging.Core.Caching.HashAlgorithms;
using Surging.Core.Caching.Interfaces;
using Surging.Core.CPlatform.Cache;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Caching.RedisCache
{
    [IdentifyCache(name: CacheTargetType.Redis)]
    public class RedisProvider : ICacheProvider
    {
        #region 字段
        private readonly Lazy<RedisContext> _context;
        private Lazy<long> _defaultExpireTime;
        private const double ExpireTime = 60D;
        private string _keySuffix;
        private Lazy<int> _connectTimeout;
        private readonly Lazy<ICacheClient<IDatabase>> _cacheClient;
        private readonly IAddressResolver addressResolver;
        #endregion

        #region 构造函数

        public RedisProvider(string appName)
        {
            _context = new Lazy<RedisContext>(() => {
                if (CacheContainer.IsRegistered<RedisContext>(appName))
                    return CacheContainer.GetService<RedisContext>(appName);
                else
                    return CacheContainer.GetInstances<RedisContext>(appName);
            });
            _keySuffix = appName;
            _defaultExpireTime = new Lazy<long>(() => long.Parse(_context.Value._defaultExpireTime));
            _connectTimeout = new Lazy<int>(() => int.Parse(_context.Value._connectTimeout));
            if (CacheContainer.IsRegistered<ICacheClient<IDatabase>>(CacheTargetType.Redis.ToString()))
            {
                addressResolver = CacheContainer.GetService<IAddressResolver>();
                _cacheClient = new Lazy<ICacheClient<IDatabase>>(() => CacheContainer.GetService<ICacheClient<IDatabase>>(CacheTargetType.Redis.ToString()));
            }
            else
                _cacheClient = new Lazy<ICacheClient<IDatabase>>(() => CacheContainer.GetInstances<ICacheClient<IDatabase>>(CacheTargetType.Redis.ToString()));
        }

        public RedisProvider()
        {

        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 添加K/V值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public void Add(string key, object value)
        {
            this.Add(key, value, TimeSpan.FromSeconds(ExpireTime));
        }

        /// <summary>
        /// 异步添加K/V值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public void AddAsync(string key, object value)
        {
            this.AddTaskAsync(key, value, TimeSpan.FromSeconds(ExpireTime));
        }

        /// <summary>
        /// 添加k/v值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="defaultExpire">默认配置失效时间</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public void Add(string key, object value, bool defaultExpire)
        {
            this.Add(key, value, TimeSpan.FromSeconds(defaultExpire ? DefaultExpireTime : ExpireTime));
        }

        /// <summary>
        /// 异步添加K/V值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="defaultExpire">默认配置失效时间</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public void AddAsync(string key, object value, bool defaultExpire)
        {
            this.AddTaskAsync(key, value, TimeSpan.FromSeconds(defaultExpire ? DefaultExpireTime : ExpireTime));
        }

        /// <summary>
        /// 添加k/v值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="numOfMinutes">默认配置失效时间</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public void Add(string key, object value, long numOfMinutes)
        {
            this.Add(key, value, TimeSpan.FromMinutes(numOfMinutes));
        }


        /// <summary>
        /// 异步添加K/V值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="numOfMinutes">默认配置失效时间</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public void AddAsync(string key, object value, long numOfMinutes)
        {
            this.AddTaskAsync(key, value, TimeSpan.FromMinutes(numOfMinutes));
        }


        /// <summary>
        /// 添加k/v值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="timeSpan">配置时间间隔</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public void Add(string key, object value, TimeSpan timeSpan)
        {
            var node = GetRedisNode(key);
            var redis = GetRedisClient(new RedisEndpoint()
            {
                DbIndex = int.Parse(node.Db),
                Host = node.Host,
                Password = node.Password,
                Port = int.Parse(node.Port),
                MinSize = int.Parse(node.MinSize),
                MaxSize = int.Parse(node.MaxSize),
            });
            redis.Set(GetKeySuffix(key), value, timeSpan);
        }

        /// <summary>
        /// 异步添加K/V值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="timeSpan">配置时间间隔</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public void AddAsync(string key, object value, TimeSpan timeSpan)
        {
            this.AddTaskAsync(key, value, timeSpan);
        }

        /// <summary>
        /// 根据KEY键集合获取返回对象集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="keys">KEY值集合</param>
        /// <returns>需要返回的对象集合</returns>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public IDictionary<string, T> Get<T>(IEnumerable<string> keys)
        {
            IDictionary<string, T> result = null;
            foreach (var key in keys)
            {

                var node = GetRedisNode(key);
                var redis = GetRedisClient(new RedisEndpoint()
                {
                    DbIndex = int.Parse(node.Db),
                    Host = node.Host,
                    Password = node.Password,
                    Port = int.Parse(node.Port),
                    MinSize = int.Parse(node.MinSize),
                    MaxSize = int.Parse(node.MaxSize),
                });
                result.Add(key, redis.Get<T>(key));
            }
            return result;
        }

        /// <summary>
        /// 根据KEY键集合异步获取返回对象集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="keys">KEY值集合</param>
        /// <returns>需要返回的对象集合</returns>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public async Task<IDictionary<string, T>> GetAsync<T>(IEnumerable<string> keys)
        {
            IDictionary<string, T> result = null;
            foreach (var key in keys)
            {

                var node = GetRedisNode(key);
                var redis = GetRedisClient(new RedisEndpoint()
                {
                    DbIndex = int.Parse(node.Db),
                    Host = node.Host,
                    Password = node.Password,
                    Port = int.Parse(node.Port),
                    MinSize = int.Parse(node.MinSize),
                    MaxSize = int.Parse(node.MaxSize),
                });
                result.Add(key, await redis.GetAsync<T>(key));
            }
            return result;
        }

        /// <summary>
        /// 根据KEY键获取返回对象
        /// </summary>
        /// <param name="key">KEY值</param>
        /// <returns>需要返回的对象</returns>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public object Get(string key)
        {
            var o = this.Get<object>(key);
            return o;
        }

        /// <summary>
        /// 根据KEY异步获取返回对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<object> GetAsync(string key)
        {
            var result = await this.GetTaskAsync<object>(key);
            return result;
        }

        /// <summary>
        /// 根据KEY键获取返回指定的类型对象
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="key">KEY值</param>
        /// <returns>需要返回的对象</returns>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public T Get<T>(string key)
        {
            var node = GetRedisNode(key);
            var result = default(T);
            var redis = GetRedisClient(new RedisEndpoint()
            {
                DbIndex = int.Parse(node.Db),
                Host = node.Host,
                Password = node.Password,
                Port = int.Parse(node.Port),
                MinSize = int.Parse(node.MinSize),
                MaxSize = int.Parse(node.MaxSize),
            });
            result = redis.Get<T>(GetKeySuffix(key));
            return result;
        }



        /// <summary>
        /// 根据KEY异步获取指定的类型对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string key)
        {
            var node = GetRedisNode(key);
            var redis = GetRedisClient(new RedisEndpoint()
            {
                DbIndex = int.Parse(node.Db),
                Host = node.Host,
                Password = node.Password,
                Port = int.Parse(node.Port),
                MinSize = int.Parse(node.MinSize),
                MaxSize = int.Parse(node.MaxSize),
            });

            var result = await Task.Run(() => redis.Get<T>(GetKeySuffix(key)));
            return result;

        }

        /// <summary>
        /// 根据KEY键获取转化成指定的对象，指示获取转化是否成功的返回值
        /// </summary>
        /// <param name="key">KEY键</param>
        /// <param name="obj">需要转化返回的对象</param>
        /// <returns>是否成功</returns>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public bool GetCacheTryParse(string key, out object obj)
        {
            obj = null;
            var o = this.Get<object>(key);
            obj = o;
            return o != null;
        }

        /// <summary>
        /// 根据KEY键删除缓存
        /// </summary>
        /// <param name="key">KEY键</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public void Remove(string key)
        {
            var node = GetRedisNode(key);
            var redis = GetRedisClient(new RedisEndpoint()
            {
                DbIndex = int.Parse(node.Db),
                Host = node.Host,
                Password = node.Password,
                Port = int.Parse(node.Port),
                MinSize = int.Parse(node.MinSize),
                MaxSize = int.Parse(node.MaxSize),
            });
            redis.Remove(GetKeySuffix(key));

        }

        /// <summary>
        /// 根据KEY键异步删除缓存
        /// </summary>
        /// <param name="key">KEY键</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public void RemoveAsync(string key)
        {
            this.RemoveTaskAsync(key);
        }

        public long DefaultExpireTime
        {
            get
            {
                return _defaultExpireTime.Value;
            }
            set
            {
                _defaultExpireTime = new Lazy<long>(() => value);
            }

        }

        public string KeySuffix
        {
            get
            {
                return _keySuffix;
            }
            set
            {
                _keySuffix = value;
            }
        }


        public int ConnectTimeout
        {
            get
            {
                return _connectTimeout.Value;
            }
            set
            {
                _connectTimeout = new Lazy<int>(() => value);
            }
        }
        #endregion

        #region 私有方法

        private IDatabase GetRedisClient(CacheEndpoint info)
        {
            return
                _cacheClient.Value
                    .GetClient(info, ConnectTimeout);
        }

        private ConsistentHashNode GetRedisNode(string item)
        {
            if (addressResolver != null)
            {
                return addressResolver.Resolver($"{KeySuffix}.{CacheTargetType.Redis.ToString()}", item).Result;
            }
            else
            {
                ConsistentHash<ConsistentHashNode> hash;
                _context.Value.dicHash.TryGetValue(CacheTargetType.Redis.ToString(), out hash);
                return hash != null ? hash.GetItemNode(item) : default(ConsistentHashNode);
            }
        }

        private async Task<T> GetTaskAsync<T>(string key)
        {
            return await Task.Run(() => this.Get<T>(key));
        }

        private async void AddTaskAsync(string key, object value, TimeSpan timeSpan)
        {
            await Task.Run(() => this.Add(key, value, timeSpan));
        }

        private async void RemoveTaskAsync(string key)
        {
            await Task.Run(() => this.Remove(key));
        }

        private string GetKeySuffix(string key)
        {
            return string.IsNullOrEmpty(KeySuffix) ? key : string.Format("_{0}_{1}", KeySuffix, key);
        }

        public async Task<bool> ConnectionAsync(CacheEndpoint endpoint)
        {
            var connection = await _cacheClient
                 .Value.ConnectionAsync(endpoint, ConnectTimeout);
            return connection;
        }

        #endregion


    }
}
