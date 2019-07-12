using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Caching.RedisCache
{
    /// <summary>
    /// Defines the <see cref="StackExchangeRedisExtensions" />
    /// </summary>
    public static class StackExchangeRedisExtensions
    {
        #region 方法

        /// <summary>
        /// The Get
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache">The cache<see cref="IDatabase"/></param>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="T"/></returns>
        public static T Get<T>(this IDatabase cache, string key)
        {
            return Deserialize<T>(cache.StringGet(key));
        }

        /// <summary>
        /// The Get
        /// </summary>
        /// <param name="cache">The cache<see cref="IDatabase"/></param>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="object"/></returns>
        public static object Get(this IDatabase cache, string key)
        {
            return Deserialize<object>(cache.StringGet(key));
        }

        /// <summary>
        /// The GetAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache">The cache<see cref="IDatabase"/></param>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        public static async Task<T> GetAsync<T>(this IDatabase cache, string key)
        {
            return Deserialize<T>(await cache.StringGetAsync(key));
        }

        /// <summary>
        /// The GetAsync
        /// </summary>
        /// <param name="cache">The cache<see cref="IDatabase"/></param>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="Task{object}"/></returns>
        public static async Task<object> GetAsync(this IDatabase cache, string key)
        {
            return Deserialize<object>(await cache.StringGetAsync(key));
        }

        /// <summary>
        /// The GetMany
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache">The cache<see cref="IDatabase"/></param>
        /// <param name="cacheKeys">The cacheKeys<see cref="IEnumerable{string}"/></param>
        /// <returns>The <see cref="IDictionary{string, T}"/></returns>
        public static IDictionary<string, T> GetMany<T>(this IDatabase cache, IEnumerable<string> cacheKeys)
        {
            var arrayKeys = cacheKeys.ToArray();
            var result = new Dictionary<string, T>();
            var keys = new RedisKey[cacheKeys.Count()];
            for (var i = 0; i < arrayKeys.Count(); i++)
            {
                keys[i] = arrayKeys[i];
            }
            var values = cache.StringGet(keys);
            for (var i = 0; i < values.Length; i++)
            {
                result.Add(keys[i], Deserialize<T>(values[i]));
            }
            return result;
        }

        /// <summary>
        /// The GetManyAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cache">The cache<see cref="IDatabase"/></param>
        /// <param name="cacheKeys">The cacheKeys<see cref="IEnumerable{string}"/></param>
        /// <returns>The <see cref="Task{IDictionary{string, T}}"/></returns>
        public static async Task<IDictionary<string, T>> GetManyAsync<T>(this IDatabase cache, IEnumerable<string> cacheKeys)
        {
            var arrayKeys = cacheKeys.ToArray();
            var result = new Dictionary<string, T>();
            var keys = new RedisKey[cacheKeys.Count()];
            for (var i = 0; i < arrayKeys.Count(); i++)
            {
                keys[i] = arrayKeys[i];
            }
            var values = await cache.StringGetAsync(keys);
            for (var i = 0; i < values.Length; i++)
            {
                result.Add(keys[i], Deserialize<T>(values[i]));
            }
            return result;
        }

        /// <summary>
        /// The Remove
        /// </summary>
        /// <param name="cache">The cache<see cref="IDatabase"/></param>
        /// <param name="key">The key<see cref="string"/></param>
        public static void Remove(this IDatabase cache, string key)
        {
            cache.KeyDelete(key);
        }

        /// <summary>
        /// The RemoveAsync
        /// </summary>
        /// <param name="cache">The cache<see cref="IDatabase"/></param>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public static async Task RemoveAsync(this IDatabase cache, string key)
        {
            await cache.KeyDeleteAsync(key);
        }

        /// <summary>
        /// The Set
        /// </summary>
        /// <param name="cache">The cache<see cref="IDatabase"/></param>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        public static void Set(this IDatabase cache, string key, object value)
        {
            cache.StringSet(key, Serialize(value));
        }

        /// <summary>
        /// The Set
        /// </summary>
        /// <param name="cache">The cache<see cref="IDatabase"/></param>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        /// <param name="timeSpan">The timeSpan<see cref="TimeSpan"/></param>
        public static void Set(this IDatabase cache, string key, object value, TimeSpan timeSpan)
        {
            cache.StringSet(key, Serialize(value), timeSpan);
        }

        /// <summary>
        /// The SetAsync
        /// </summary>
        /// <param name="cache">The cache<see cref="IDatabase"/></param>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public static async Task SetAsync(this IDatabase cache, string key, object value)
        {
            await cache.StringSetAsync(key, Serialize(value));
        }

        /// <summary>
        /// The SetAsync
        /// </summary>
        /// <param name="cache">The cache<see cref="IDatabase"/></param>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        /// <param name="timeSpan">The timeSpan<see cref="TimeSpan"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public static async Task SetAsync(this IDatabase cache, string key, object value, TimeSpan timeSpan)
        {
            await cache.StringSetAsync(key, Serialize(value), timeSpan);
        }

        /// <summary>
        /// The Deserialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream">The stream<see cref="byte[]"/></param>
        /// <returns>The <see cref="T"/></returns>
        internal static T Deserialize<T>(byte[] stream)
        {
            if (stream == null)
            {
                return default(T);
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream(stream))
            {
                T result = (T)binaryFormatter.Deserialize(memoryStream);
                return result;
            }
        }

        /// <summary>
        /// The Serialize
        /// </summary>
        /// <param name="o">The o<see cref="object"/></param>
        /// <returns>The <see cref="byte[]"/></returns>
        internal static byte[] Serialize(object o)
        {
            if (o == null)
            {
                return null;
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, o);
                byte[] objectDataAsStream = memoryStream.ToArray();
                return objectDataAsStream;
            }
        }

        #endregion 方法
    }
}