using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ProxyGenerator
{
    #region 接口

    /// <summary>
    /// 代理服务接口
    /// </summary>
    public interface IServiceProxyProvider
    {
        #region 方法

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="routePath">The routePath<see cref="string"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        Task<T> Invoke<T>(IDictionary<string, object> parameters, string routePath);

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="routePath">The routePath<see cref="string"/></param>
        /// <param name="serviceKey">The serviceKey<see cref="string"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        Task<T> Invoke<T>(IDictionary<string, object> parameters, string routePath, string serviceKey);

        #endregion 方法
    }

    #endregion 接口
}