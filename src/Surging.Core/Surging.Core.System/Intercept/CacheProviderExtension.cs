using Newtonsoft.Json;
using Surging.Core.Caching;
using Surging.Core.CPlatform.Cache;
using System;
using System.Threading.Tasks;

namespace Surging.Core.System.Intercept
{
    public static class CacheProviderExtension
    {
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
    }
}
