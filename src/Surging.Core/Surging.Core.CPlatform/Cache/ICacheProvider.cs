using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Cache
{
    public interface ICacheProvider
    {
        Task<bool> ConnectionAsync(CacheEndpoint endpoint);
        void Add(string key, object value);
        void AddAsync(string key, object value);
        void Add(string key, object value, bool defaultExpire);
        void AddAsync(string key, object value, bool defaultExpire);
        void Add(string key, object value, long numOfMinutes);
        void AddAsync(string key, object value, long numOfMinutes);
        void Add(string key, object value, TimeSpan timeSpan);
        void AddAsync(string key, object value, TimeSpan timeSpan);

        IDictionary<string, T> Get<T>(IEnumerable<string> keys);
        Task<IDictionary<string, T>> GetAsync<T>(IEnumerable<string> keys);
        object Get(string key);
        Task<object> GetAsync(string key);
        T Get<T>(string key);
        Task<T> GetAsync<T>(string key);
        bool GetCacheTryParse(string key, out object obj);
        void Remove(string key);
        void RemoveAsync(string key);
        long DefaultExpireTime { get; set; }
        string KeySuffix { get; set; }
    }
}


