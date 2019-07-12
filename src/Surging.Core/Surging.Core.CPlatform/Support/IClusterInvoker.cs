using System.Collections.Generic;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Support
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IClusterInvoker" />
    /// </summary>
    public interface IClusterInvoker
    {
        #region 方法

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <param name="_serviceKey">The _serviceKey<see cref="string"/></param>
        /// <param name="decodeJOject">The decodeJOject<see cref="bool"/></param>
        /// <returns>The <see cref="Task"/></returns>
        Task Invoke(IDictionary<string, object> parameters, string serviceId, string _serviceKey, bool decodeJOject);

        /// <summary>
        /// The Invoke
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters">The parameters<see cref="IDictionary{string, object}"/></param>
        /// <param name="serviceId">The serviceId<see cref="string"/></param>
        /// <param name="_serviceKey">The _serviceKey<see cref="string"/></param>
        /// <param name="decodeJOject">The decodeJOject<see cref="bool"/></param>
        /// <returns>The <see cref="Task{T}"/></returns>
        Task<T> Invoke<T>(IDictionary<string, object> parameters, string serviceId, string _serviceKey, bool decodeJOject);

        #endregion 方法
    }

    #endregion 接口
}