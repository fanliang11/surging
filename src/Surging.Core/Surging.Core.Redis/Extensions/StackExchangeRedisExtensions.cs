using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Redis.Extensions
{
   public static class StackExchangeRedisExtensions
    {
        public static T Get<T>(this IDatabase cache, string key)
        {
            return Deserialize<T>(cache.StringGet(key));
        }

        public static void Remove(this IDatabase cache, string key)
        {
            cache.KeyDelete(key);
        }

        public static async Task RemoveAsync(this IDatabase cache, string key)
        {
            await cache.KeyDeleteAsync(key);
        }

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

        public static async Task<T> GetAsync<T>(this IDatabase cache, string key)
        {
            return Deserialize<T>(await cache.StringGetAsync(key));
        }

        public static object Get(this IDatabase cache, string key)
        {
            return Deserialize<object>(cache.StringGet(key));
        }

        public static async Task<object> GetAsync(this IDatabase cache, string key)
        {
            return Deserialize<object>(await cache.StringGetAsync(key));
        }

        public static void Set(this IDatabase cache, string key, object value)
        {
            cache.StringSet(key, Serialize(value));
        }

        public static async Task SetAsync(this IDatabase cache, string key, object value)
        {
            await cache.StringSetAsync(key, Serialize(value));
        }

        public static void Set(this IDatabase cache, string key, object value, TimeSpan timeSpan)
        {
            cache.StringSet(key, Serialize(value), timeSpan);
        }

        public static async Task SetAsync(this IDatabase cache, string key, object value, TimeSpan timeSpan)
        {
            await cache.StringSetAsync(key, Serialize(value), timeSpan);
        }


        static byte[] Serialize(object o)
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

        static T Deserialize<T>(byte[] stream)
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
    }
}
