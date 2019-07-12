using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Server
{
    /// <summary>
    /// 服务条目。
    /// </summary>
    public class ServiceEntry
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Attributes
        /// </summary>
        public List<Attribute> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the Descriptor
        /// 服务描述符。
        /// </summary>
        public ServiceDescriptor Descriptor { get; set; }

        /// <summary>
        /// Gets or sets the Func
        /// 执行委托。
        /// </summary>
        public Func<string, IDictionary<string, object>, Task<object>> Func { get; set; }

        /// <summary>
        /// Gets or sets the MethodName
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// Gets or sets the RoutePath
        /// </summary>
        public string RoutePath { get; set; }

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public Type Type { get; set; }

        #endregion 属性
    }
}