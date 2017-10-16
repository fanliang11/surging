using Surging.Core.Caching.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using System.Collections;
using System.Text.RegularExpressions;

namespace Surging.Core.Caching.NetCache
{
    public class MemoryCache
    {
        private static readonly ConcurrentDictionary<string, Tuple<string, object, DateTime>> cache = new ConcurrentDictionary<string, Tuple<string, object, DateTime>>();
        private const  int taskInterval = 5;

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

        public static int Count
        {
            get
            {
                return cache.Count;
            }
        }
        
        /// <summary>
        /// 获得一个Cache对象
        /// </summary>
        /// <param name="key">标识</param>
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

        public static bool GetCacheTryParse(string key, out object obj)
        {
            Check.CheckCondition(() => string.IsNullOrEmpty(key), "key");
            obj = Get(key);
            return (obj != null);
        }
        
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
        /// 是否存在缓存
        /// </summary>
        /// <param name="key">标识</param>
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

        public static void Set(string key, object value, double cacheSecond)
        {
            DateTime cacheTime = DateTime.Now.AddSeconds(cacheSecond);
            var cacheValue = new Tuple<string, object, DateTime>(key, value, cacheTime);
            cache.AddOrUpdate(key, cacheValue, (v, oldValue) => cacheValue);
        }

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

        public static void Remove(string key)
        {
            Tuple<string, object, DateTime> item;
            cache.TryRemove(key,out item);
        }

        public static void Dispose()
        {
            cache.Clear();
        }

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
    }
}
