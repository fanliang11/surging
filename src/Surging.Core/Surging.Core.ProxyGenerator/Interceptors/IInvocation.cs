using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ProxyGenerator.Interceptors
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IInvocation" />
    /// </summary>
    public interface IInvocation
    {
        #region 属性

        /// <summary>
        /// Gets the Arguments
        /// </summary>
        IDictionary<string, object> Arguments { get; }

        /// <summary>
        /// Gets the Proxy
        /// </summary>
        object Proxy { get; }

        /// <summary>
        /// Gets the ReturnType
        /// </summary>
        Type ReturnType { get; }

        /// <summary>
        /// Gets or sets the ReturnValue
        /// </summary>
        object ReturnValue { get; set; }

        /// <summary>
        /// Gets the ServiceId
        /// </summary>
        string ServiceId { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Proceed
        /// </summary>
        /// <returns>The <see cref="Task"/></returns>
        Task Proceed();

        /// <summary>
        /// The SetArgumentValue
        /// </summary>
        /// <param name="index">The index<see cref="int"/></param>
        /// <param name="value">The value<see cref="object"/></param>
        void SetArgumentValue(int index, object value);

        #endregion 方法
    }

    #endregion 接口
}