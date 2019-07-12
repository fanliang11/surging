using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Cache
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="ICacheProvider" />
    /// </summary>
    public interface ICacheProvider
    {
        #region 属性

        /// <summary>
        /// Gets or sets the DefaultExpireTime
        /// </summary>
        long DefaultExpireTime { get; set; }

        /// <summary>
        /// Gets or sets the KeySuffix
        /// </summary>
        string KeySuffix { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Add
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        void Add(string key, object value);

        /// <summary>
        /// The Add
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        /// <param name="defaultExpire">The defaultExpire<see cref="bool"/></param>
        void Add(string key, object value, bool defaultExpire);

        /// <summary>
        /// The Add
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        /// <param name="numOfMinutes">The numOfMinutes<see cref="long"/></param>
        void Add(string key, object value, long numOfMinutes);

        /// <summary>
        /// The Add
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        /// <param name="timeSpan">The timeSpan<see cref="TimeSpan"/></param>
        void Add(string key, object value, TimeSpan timeSpan);

        /// <summary>
        /// The AddAsync
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        void AddAsync(string key, object value);

        /// <summary>
        /// The AddAsync
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        /// <param name="defaultExpire">The defaultExpire<see cref="bool"/></param>
        void AddAsync(string key, object value, bool defaultExpire);

        /// <summary>
        /// The AddAsync
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        /// <param name="numOfMinutes">The numOfMinutes<see cref="long"/></param>
        void AddAsync(string key, object value, long numOfMinutes);

        /// <summary>
        /// The AddAsync
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        /// <param name="timeSpan">The timeSpan<see cref="TimeSpan"/></param>
        void AddAsync(string key, object value, TimeSpan timeSpan);

        /// <summary>
        /// The ConnectionAsync
        /// </summary>
        /// <param name="endpoint">The endpoint<see cref="CacheEndpoint"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        Task<bool> ConnectionAsync(CacheEndpoint endpoint);

        /// <summary>
        /// The Get
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys">The keys<see cref="IEnumerable{string}"/></param>
        /// <returns>The <see cref="IDictionary{string, T}"/></returns>
        IDictionary<string, T> Get<T>(IEnumerable<string> keys);

        /// <summary>
        /// The Get
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="object"/></returns>
        object Get(string key);

        /// <summary>
        /// The Get
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="T"/></returns>
        T Get<T>(string key);

        /// <summary>
        /// The GetAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys">The keys<see cref="IEnumerable{string}"/></param>
        /// <returns>The <see cref="Task{IDictionary{string, T}}"/></returns>
        Task<IDictionary<string, T>> GetAsync<T>(IEnumerable<string> keys);

        /// <summary>
        /// The GetAsync
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="Task{object}"/></returns>
        Task<object> GetAsync(string key);

        /// <summary>
        /// The GetAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key<see cref="string"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// The GetCacheTryParse
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        /// <param name="obj">The obj<see cref="object"/></param>
        /// <returns>The <see cref="bool"/></returns>
        bool GetCacheTryParse(string key, out object obj);

        /// <summary>
        /// The Remove
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        void Remove(string key);

        /// <summary>
        /// The RemoveAsync
        /// </summary>
        /// <param name="key">The key<see cref="string"/></param>
        void RemoveAsync(string key);

        #endregion 方法
    }

    #endregion 接口
}