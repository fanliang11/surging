using StackExchange.Redis;
using Surging.Core.CPlatform.Cache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Caching.Interfaces
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="ICacheClient{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICacheClient<T>
    {
        #region 方法

        /// <summary>
        /// The ConnectionAsync
        /// </summary>
        /// <param name="endpoint">The endpoint<see cref="CacheEndpoint"/></param>
        /// <param name="connectTimeout">The connectTimeout<see cref="int"/></param>
        /// <returns>The <see cref="Task{bool}"/></returns>
        Task<bool> ConnectionAsync(CacheEndpoint endpoint, int connectTimeout);

        /// <summary>
        /// The GetClient
        /// </summary>
        /// <param name="info">The info<see cref="CacheEndpoint"/></param>
        /// <param name="connectTimeout">The connectTimeout<see cref="int"/></param>
        /// <returns>The <see cref="T"/></returns>
        T GetClient(CacheEndpoint info, int connectTimeout);

        #endregion 方法
    }

    #endregion 接口
}