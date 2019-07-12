using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Surging.Core.Common
{
    /// <summary>
    /// Defines the <see cref="ApiResult{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public class ApiResult<T>
    {
        #region 属性

        /// <summary>
        /// Gets or sets the StatusCode
        /// </summary>
        [DataMember]
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the Value
        /// </summary>
        [DataMember]
        public T Value { get; set; }

        #endregion 属性
    }
}