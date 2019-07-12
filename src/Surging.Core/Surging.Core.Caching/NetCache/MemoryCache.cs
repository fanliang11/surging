using Surging.Core.Caching.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Surging.Core.Caching.NetCache
{
    /// <summary>
    /// Defines the <see cref="MemoryCache" />
    /// </summary>
    public class MemoryCache
    {
        #region 常量

        /// <summary>
        /// Defines the taskInterval
        /// </summary>
        private const int taskInterval = 5;

        #endregion 常量

        #region 字段

        /// <summary>
        /// Defines the cache
        /// </summary>
        private static readonly ConcurrentDictionary<string, Tuple<string, object, DateTime>> cache = new ConcurrentDictionary<string, Tuple<string, object, DateTime>>();

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes static members of the <see cref="MemoryCache"/> class.
        /// </summary>
        static MemoryCache()
        {
            try
            {
                GCThreadProvider.AddThread(new ParameterizedThreadStart(Collect));
            }
            catch (Exception err)
            {
                throw new CacheException(err.Message, err);
            }
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Count
        /// </summary>
        public static int Count
        {
            get
            {
                return cache.Count;
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// 是否存在缓存
        /// </summary>
        /// <param name="key">标识</param>
        /// <param name="value">The value<see cref="object"/></param>
        /// <returns></returns>
        public static bool Contains(string key, out object value)
        {
            bool isSuccess = false;
            Tuple<string, object, DateTime> item;
            value = null;
            if (cache.TryGetValue(key, out item))
            {
                value = item.Item2;
                isSuccess = item.Item3 > DateTime.Now;
            }
            return isSuccess;
        }

        /// <summary>
        /// The Dispose
        /// </summary>
        public static void Dispose()
        {
            cache.Clear();
        }

        /// <summary>
        /// The Get
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys">The keys<see cref="IEnumerable{string}"/></param>
        /// <returns>The <see cref="IDictionary{string, T}"/></returns>
        public static IDictionary<string, T> Get<T>(IEnumerable<string> keys)
        {
            if (keys == null)
            {
                return new Dictionary<string, T>();
            }
            var dictionary = new Dictionary<string, T>();
            IEnumerator<string> enumerator = keys.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string current = enumerator.Current;
                object obj2 = Get(current);
                if (obj2 is T)
                {
                    dictionary.Add(current, (T)obj2);
                }
            }
            return dictionary;
        }

        /// <summary>
        /// 获得一个Cache对象
        /// </summary>
        /// <param name="key">标识</param>
        /// <returns>The <see cref="object"/></returns>
        public static object Get(string key)
        {
            Check.CheckCondition(() => string.IsNullOrEmpty(key), "key");
            object result;
            if (Contains(key, out result))
            {
                return result;
            }
            return null;
        }

        /// <summary>
        /// The Get
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="T"/></returns>
        public static T Get<T>(string key)
        {
            Check.CheckCondition(() => string.IsNullOrEmpty(key), "key");
            object obj2 = Get(key);
            if (obj2 is T)
            {
                return (T)obj2;
            }
            return default(T);
        }

        /// <summary>
        /// The GetCacheTryParse
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="obj">The obj<see cref="object"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static bool GetCacheTryParse(string key, out object obj)
        {
            Check.CheckCondition(() => string.IsNullOrEmpty(key), "key");
            obj = Get(key);
            return (obj != null);
        }

        /// <summary>
        /// The Remove
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        public static void Remove(string key)
        {
            Tuple<string, object, DateTime> item;
            cache.TryRemove(key, out item);
        }

        /// <summary>
        /// The RemoveByPattern
        /// </summary>
        /// <param name="pattern">The pattern<see cref="string"/></param>
        public static void RemoveByPattern(string pattern)
        {
            var enumerator = cache.GetEnumerator();
            Regex regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            while (enumerator.MoveNext())
            {
                string input = enumerator.Current.Key.ToString();
                if (regex.IsMatch(input))
                {
                    Remove(input);
                }
            }
        }

        /// <summary>
        /// The Set
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        /// <param name="cacheSecond">The cacheSecond<see cref="double"/></param>
        public static void Set(string key, object value, double cacheSecond)
        {
            DateTime cacheTime = DateTime.Now.AddSeconds(cacheSecond);
            var cacheValue = new Tuple<string, object, DateTime>(key, value, cacheTime);
            cache.AddOrUpdate(key, cacheValue, (v, oldValue) => cacheValue);
        }

        /// <summary>
        /// The Collect
        /// </summary>
        /// <param name="threadID">The threadID<see cref="object"/></param>
        private static void Collect(object threadID)
        {
            while (true)
            {
                try
                {
                    var cacheValues = cache.Values;
                    cacheValues = cacheValues.OrderBy(p => p.Item3).ToList();
                    foreach (var cacheValue in cacheValues)
                    {
                        if ((cacheValue.Item3 - DateTime.Now).Seconds <= 0)
                        {
                            Tuple<string, object, DateTime> item;
                            cache.TryRemove(cacheValue.Item1, out item);
                        }
                    }
                    Thread.Sleep(taskInterval * 60 * 1000);
                }
                catch
                {
                    Dispose();
                    GCThreadProvider.AddThread(new ParameterizedThreadStart(Collect));
                }
            }
        }

        #endregion 方法
    }
}