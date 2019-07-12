using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    /// <summary>
    /// Defines the <see cref="ServiceRouteAttribute" />
    /// </summary>
    public class ServiceRouteAttribute : Attribute
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRouteAttribute"/> class.
        /// </summary>
        /// <param name="template">The template<see cref="string"/></param>
        public ServiceRouteAttribute(string template)
        {
            Template = template;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Template
        /// </summary>
        public string Template { get; }

        #endregion 属性
    }
}