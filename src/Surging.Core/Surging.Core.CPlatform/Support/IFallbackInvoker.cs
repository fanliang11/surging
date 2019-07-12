using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Support
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IFallbackInvoker" />
    /// </summary>
    public interface IFallbackInvoker
    {
        #region 方法

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <param name="_serviceKey">The _serviceKey<see cref="string"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task Invoke(IDictionary<string, object> parameters, string serviceId, string _serviceKey);

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <param name="_serviceKey">The _serviceKey<see cref="string"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        Task<T> Invoke<T>(IDictionary<string, object> parameters, string serviceId, string _serviceKey);

        #endregion 方法
    }

    #endregion 接口
}