using Newtonsoft.Json;
using Surging.Core.Caching;
using Surging.Core.CPlatform.Cache;
using System;
using System.Threading.Tasks;

namespace Surging.Core.System.Intercept
{
    /// <summary>
    /// Defines the <see cref="CacheProviderExtension" />
    /// </summary>
    public static class CacheProviderExtension
    {
        #region 方法

        /// <summary>
        /// The GetFromCacheFirst
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheProvider">The cacheProvider<see cref="ICacheProvider"/></param>
        /// <param name="l2cacheProvider">The l2cacheProvider<see cref="ICacheProvider"/></param>
        /// <param name="l2Key">The l2Key<see cref="string"/></param>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="getFromPersistence">The getFromPersistence<see cref="Func{Task{T}}"/></param>
        /// <param name="returnType">The returnType<see cref="Type"/></param>
        /// <param name="storeTime">The storeTime<see cref="long?"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        public static async Task<T> GetFromCacheFirst<T>(this ICacheProvider cacheProvider, ICacheProvider l2cacheProvider, string l2Key, string key, Func<Task<T>> getFromPersistence, Type returnType, long? storeTime = null) where T : class
        {
            object returnValue;
            try
            {
                var signJson = cacheProvider.Get<string>(key);
                if (string.IsNullOrEmpty(signJson) || signJson == "\"[]\"")
                {
                    returnValue = await getFromPersistence();
                    if (returnValue != null)
                    {
                        var resultJson = JsonConvert.SerializeObject(returnValue);
                        var sign = Guid.NewGuid();
                        signJson = JsonConvert.SerializeObject(sign);
                        if (l2Key == key)
                        {
                            SetCache(cacheProvider, key, signJson, storeTime);
                        }
                        SetCache(l2cacheProvider, l2Key, new ValueTuple<string, string>(signJson, resultJson), storeTime);
                    }
                }
                else
                {
                    var l2Cache = l2cacheProvider.Get<ValueTuple<string, string>>(l2Key);
                    if (l2Cache == default || l2Cache.Item1 != signJson)
                    {
                        returnValue = await getFromPersistence();
                        if (returnValue != null)
                        {
                            var resultJson = JsonConvert.SerializeObject(returnValue);
                            SetCache(l2cacheProvider, l2Key, new ValueTuple<string, string>(signJson, resultJson), storeTime);
                        }
                    }
                    else
                    {
                        returnValue = JsonConvert.DeserializeObject(l2Cache.Item2, returnType);
                    }
                }
                return returnValue as T;
            }
            catch
            {
                returnValue = await getFromPersistence();
                return returnValue as T;
            }
        }

        /// <summary>
        /// The GetFromCacheFirst
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheProvider">The cacheProvider<see cref="ICacheProvider"/></param>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="getFromPersistence">The getFromPersistence<see cref="Func{Task{T}}"/></param>
        /// <param name="returnType">The returnType<see cref="Type"/></param>
        /// <param name="storeTime">The storeTime<see cref="long?"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        public static async Task<T> GetFromCacheFirst<T>(this ICacheProvider cacheProvider, string key, Func<Task<T>> getFromPersistence, Type returnType, long? storeTime = null) where T : class
        {
            object returnValue;
            try
            {
                var resultJson = cacheProvider.Get<string>(key);
                if (string.IsNullOrEmpty(resultJson) || resultJson == "\"[]\"")
                {
                    returnValue = await getFromPersistence();
                    if (returnValue != null)
                    {
                        resultJson = JsonConvert.SerializeObject(returnValue);
                        if (storeTime.HasValue)
                        {
                            cacheProvider.Remove(key);
                            cacheProvider.Add(key, resultJson, storeTime.Value);
                        }
                        else
                        {
                            cacheProvider.Remove(key);
                            cacheProvider.Add(key, resultJson);
                        }
                    }
                }
                else
                {
                    returnValue = JsonConvert.DeserializeObject(resultJson, returnType);
                }
                return returnValue as T;
            }
            catch
            {
                returnValue = await getFromPersistence();
                return returnValue as T;
            }
        }

        /// <summary>
        /// The SetCache
        /// </summary>
        /// <param name="cacheProvider">The cacheProvider<see cref="ICacheProvider"/></param>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        /// <param name="numOfMinutes">The numOfMinutes<see cref="long?"/></param>
        private static void SetCache(ICacheProvider cacheProvider, string key, object value, long? numOfMinutes)
        {
            if (numOfMinutes.HasValue)
            {
                cacheProvider.Remove(key);
                cacheProvider.Add(key, value, numOfMinutes.Value);
            }
            else
            {
                cacheProvider.Remove(key);
                cacheProvider.Add(key, value);
            }
        }

        #endregion 方法
    }
}