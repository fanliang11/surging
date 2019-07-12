using Surging.Core.Caching.Interfaces;
using Surging.Core.Caching.RedisCache;
using Surging.Core.CPlatform.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Caching.NetCache
{
    /// <summary>
    /// Defines the <see cref="MemoryCacheProvider" />
    /// </summary>
    [IdentifyCache(name: CacheTargetType.MemoryCache)]
    public sealed class MemoryCacheProvider : ICacheProvider
    {
        #region 常量

        /// <summary>
        /// 配置失效时间
        /// </summary>
        private const double ExpireTime = 60D;

        #endregion 常量

        #region 字段

        /// <summary>
        /// 缓存数据上下文
        /// </summary>
        private readonly Lazy<RedisContext> _context;

        /// <summary>
        /// 默认失效时间
        /// </summary>
        private Lazy<long> _defaultExpireTime;

        /// <summary>
        /// KEY键前缀
        /// </summary>
        private string _keySuffix;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheProvider"/> class.
        /// </summary>
        public MemoryCacheProvider()
        {
            _defaultExpireTime = new Lazy<long>(() => 60);
            _keySuffix = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheProvider"/> class.
        /// </summary>
        /// <param name="appName">The appName<see cref="string"/></param>
        public MemoryCacheProvider(string appName)
        {
            _context = new Lazy<RedisContext>(() =>
            {
                if (CacheContainer.IsRegistered<RedisContext>(CacheTargetType.Redis.ToString()))
                    return CacheContainer.GetService<RedisContext>(appName);
                else
                    return CacheContainer.GetInstances<RedisContext>(appName);
            });

            _keySuffix = appName;
            _defaultExpireTime = new Lazy<long>(() => long.Parse(_context.Value._defaultExpireTime));
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the DefaultExpireTime
        /// 默认缓存失效时间
        /// </summary>
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

        /// <summary>
        /// Gets or sets the KeySuffix
        /// KEY前缀
        /// </summary>
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

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Add
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        public void Add(string key, object value)
        {
            MemoryCache.Set(GetKeySuffix(key), value, _defaultExpireTime.Value);
        }

        /// <summary>
        /// The Add
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        /// <param name="defaultExpire">The defaultExpire<see cref="bool"/></param>
        public void Add(string key, object value, bool defaultExpire)
        {
            MemoryCache.Set(GetKeySuffix(key), value, defaultExpire ? DefaultExpireTime : ExpireTime);
        }

        /// <summary>
        /// The Add
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        /// <param name="numOfMinutes">The numOfMinutes<see cref="long"/></param>
        public void Add(string key, object value, long numOfMinutes)
        {
            MemoryCache.Set(GetKeySuffix(key), value, numOfMinutes * 60);
        }

        /// <summary>
        /// The Add
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        /// <param name="timeSpan">The timeSpan<see cref="TimeSpan"/></param>
        public void Add(string key, object value, TimeSpan timeSpan)
        {
            MemoryCache.Set(GetKeySuffix(key), value, timeSpan.TotalSeconds);
        }

        /// <summary>
        /// The AddAsync
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        public async void AddAsync(string key, object value)
        {
            await Task.Run(() => MemoryCache.Set(GetKeySuffix(key), value, DefaultExpireTime));
        }

        /// <summary>
        /// The AddAsync
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        /// <param name="defaultExpire">The defaultExpire<see cref="bool"/></param>
        public async void AddAsync(string key, object value, bool defaultExpire)
        {
            await Task.Run(() => MemoryCache.Set(GetKeySuffix(key), value, defaultExpire ? DefaultExpireTime : ExpireTime));
        }

        /// <summary>
        /// The AddAsync
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        /// <param name="numOfMinutes">The numOfMinutes<see cref="long"/></param>
        public async void AddAsync(string key, object value, long numOfMinutes)
        {
            await Task.Run(() => MemoryCache.Set(GetKeySuffix(key), value, numOfMinutes * 60));
        }

        /// <summary>
        /// The AddAsync
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        /// <param name="timeSpan">The timeSpan<see cref="TimeSpan"/></param>
        public async void AddAsync(string key, object value, TimeSpan timeSpan)
        {
            await Task.Run(() => MemoryCache.Set(GetKeySuffix(key), value, timeSpan.TotalSeconds));
        }

        /// <summary>
        /// The ConnectionAsync
        /// </summary>
        /// <param name="endpoint">The endpoint<see cref="CacheEndpoint"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        public Task<bool> ConnectionAsync(CacheEndpoint endpoint)
        {
            return Task.FromResult<bool>(true);
        }

        /// <summary>
        /// The Get
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys">The keys<see cref="IEnumerable{string}"/></param>
        /// <returns>The <see cref="IDictionary{string, T}"/></returns>
        public IDictionary<string, T> Get<T>(IEnumerable<string> keys)
        {
            keys.ToList().ForEach(key => key = GetKeySuffix(key));
            return MemoryCache.Get<T>(keys);
        }

        /// <summary>
        /// The Get
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="object"/></returns>
        public object Get(string key)
        {
            return MemoryCache.Get(GetKeySuffix(key));
        }

        /// <summary>
        /// The Get
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="T"/></returns>
        public T Get<T>(string key)
        {
            return MemoryCache.Get<T>(GetKeySuffix(key));
        }

        /// <summary>
        /// The GetAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys">The keys<see cref="IEnumerable{string}"/></param>
        /// <returns>The <see cref="Task{IDictionary{string, T}}"/></returns>
        public async Task<IDictionary<string, T>> GetAsync<T>(IEnumerable<string> keys)
        {
            keys.ToList().ForEach(key => key = GetKeySuffix(key));
            var result = await Task.Run(() => MemoryCache.Get<T>(keys));
            return result;
        }

        /// <summary>
        /// The GetAsync
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="Task{object}"/></returns>
        public async Task<object> GetAsync(string key)
        {
            var result = await Task.Run(() => MemoryCache.Get(GetKeySuffix(key)));
            return result;
        }

        /// <summary>
        /// The GetAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        public async Task<T> GetAsync<T>(string key)
        {
            var result = await Task.Run(() => MemoryCache.Get<T>(GetKeySuffix(key)));
            return result;
        }

        /// <summary>
        /// The GetCacheTryParse
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="obj">The obj<see cref="object"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool GetCacheTryParse(string key, out object obj)
        {
            return MemoryCache.GetCacheTryParse(GetKeySuffix(key), out obj);
        }

        /// <summary>
        /// The Remove
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        public void Remove(string key)
        {
            MemoryCache.RemoveByPattern(GetKeySuffix(key));
        }

        /// <summary>
        /// The RemoveAsync
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        public async void RemoveAsync(string key)
        {
            await Task.Run(() => MemoryCache.Remove(GetKeySuffix(key)));
        }

        /// <summary>
        /// The GetKeySuffix
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        private string GetKeySuffix(string key)
        {
            return string.IsNullOrEmpty(KeySuffix) ? key : string.Format("_{0}_{1}", KeySuffix, key);
        }

        #endregion 方法
    }
}