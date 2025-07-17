﻿using Newtonsoft.Json;
using Surging.Core.Caching;
using Surging.Core.CPlatform.Cache;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;


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
                    returnValue = JsonSerializer.Deserialize(resultJson, returnType, JsonOption.SerializeOptions);
                }
                return returnValue as T;
            }
            catch
            {
                returnValue = await getFromPersistence();
                return returnValue as T;
            }
        }

        public static async Task<T> GetFromCacheFirst<T>(this ICacheProvider cacheProvider, ICacheProvider l2cacheProvider,string l2Key, string key, Func<Task<T>> getFromPersistence, Type returnType, long? storeTime = null) where T : class
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
                        SetCache(l2cacheProvider, l2Key, new ValueTuple<string,string>(signJson, resultJson), storeTime);
                    }
                }
                else
                {
                   var l2Cache= l2cacheProvider.Get<ValueTuple<string, string>>(l2Key);
                    if(l2Cache==default || l2Cache.Item1!=signJson)
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
                       returnValue = JsonSerializer.Deserialize(l2Cache.Item2, returnType, JsonOption.SerializeOptions);
                    }
                }
                return returnValue as T;
            }
            catch(Exception ex)
            {
                returnValue = await getFromPersistence();
                return returnValue as T;
            }
        }

        private  static void SetCache(ICacheProvider cacheProvider, string key, object value, long? numOfMinutes)
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
    }
}
