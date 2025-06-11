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
    [IdentifyCache(name: CacheTargetType.MemoryCache)]
    public sealed class MemoryCacheProvider : ICacheProvider
    {
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
        /// 配置失效时间
        /// </summary>
        private const double ExpireTime = 60D;

        /// <summary>
        /// KEY键前缀
        /// </summary>
        private string _keySuffix;

        #endregion

        #region 构造函数

        public MemoryCacheProvider(string appName)
        {
            _context = new Lazy<RedisContext>(() => {
                if (CacheContainer.IsRegistered<RedisContext>(CacheTargetType.Redis.ToString()))
                    return CacheContainer.GetService<RedisContext>(appName);
                else
                    return CacheContainer.GetInstances<RedisContext>(appName);
            });

            _keySuffix = appName;
            _defaultExpireTime = new Lazy<long>(() => long.Parse(_context.Value._defaultExpireTime));
        }

        public MemoryCacheProvider()
        {
            _defaultExpireTime = new Lazy<long>(() => 60);
            _keySuffix = string.Empty;
        }

        public void Add(string key, object value)
        {
            MemoryCache.Set(GetKeySuffix(key), value, _defaultExpireTime.Value);
        }

        public async void AddAsync(string key, object value)
        {
            await Task.Run(() => MemoryCache.Set(GetKeySuffix(key), value, DefaultExpireTime));
        }

        public void Add(string key, object value, bool defaultExpire)
        {
            MemoryCache.Set(GetKeySuffix(key), value, defaultExpire ? DefaultExpireTime : ExpireTime);
        }

        public async void AddAsync(string key, object value, bool defaultExpire)
        {
            await Task.Run(() => MemoryCache.Set(GetKeySuffix(key), value, defaultExpire ? DefaultExpireTime : ExpireTime));
        }

        public void Add(string key, object value, long numOfMinutes)
        {
            MemoryCache.Set(GetKeySuffix(key), value, numOfMinutes*60);
        }

        public async void AddAsync(string key, object value, long numOfMinutes)
        {
            await Task.Run(() => MemoryCache.Set(GetKeySuffix(key), value, numOfMinutes*60));
        }

        public void Add(string key, object value, TimeSpan timeSpan)
        {
            MemoryCache.Set(GetKeySuffix(key), value, timeSpan.TotalSeconds);
        }

        public async void AddAsync(string key, object value, TimeSpan timeSpan)
        {
            await Task.Run(() => MemoryCache.Set(GetKeySuffix(key), value, timeSpan.TotalSeconds));
        }

        public IDictionary<string, T> Get<T>(IEnumerable<string> keys)
        {
            keys.ToList().ForEach(key => key = GetKeySuffix(key));
            return MemoryCache.Get<T>(keys);
        }

        public async Task<IDictionary<string, T>> GetAsync<T>(IEnumerable<string> keys)
        {
            keys.ToList().ForEach(key => key = GetKeySuffix(key));
            var result = await Task.Run(() => MemoryCache.Get<T>(keys));
            return result;
        }

        public object Get(string key)
        {
            return MemoryCache.Get(GetKeySuffix(key));
        }

        public async Task<object> GetAsync(string key)
        {
            var result = await Task.Run(() => MemoryCache.Get(GetKeySuffix(key)));
            return result;
        }

        public T Get<T>(string key)
        {
            return MemoryCache.Get<T>(GetKeySuffix(key));
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var result = await Task.Run(() => MemoryCache.Get<T>(GetKeySuffix(key)));
            return result;
        }

        public bool GetCacheTryParse(string key, out object obj)
        {
            return MemoryCache.GetCacheTryParse(GetKeySuffix(key), out obj);
        }

        public void Remove(string key)
        {
            MemoryCache.RemoveByPattern(GetKeySuffix(key));
        }

        public async void RemoveAsync(string key)
        {
            await Task.Run(() => MemoryCache.Remove(GetKeySuffix(key)));
        }

        public Task<bool> ConnectionAsync(CacheEndpoint endpoint)
        {
            return Task.FromResult<bool>(true);
        }

        #endregion

        #region 属性

        /// <summary>
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
        #endregion

        #region 私有变量
        private string GetKeySuffix(string key)
        {
            return string.IsNullOrEmpty(KeySuffix) ? key : string.Format("_{0}_{1}", KeySuffix, key);
        }
        #endregion

    }
}
