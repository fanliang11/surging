using System;

namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    /// <summary>
    /// 服务集标记。
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceBundleAttribute : Attribute
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBundleAttribute"/> class.
        /// </summary>
        /// <param name="routeTemplate">The routeTemplate<see cref="string"/></param>
        /// <param name="isPrefix">The isPrefix<see cref="bool"/></param>
        public ServiceBundleAttribute(string routeTemplate, bool isPrefix = true)
        {
            RouteTemplate = routeTemplate;
            IsPrefix = isPrefix;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets a value indicating whether IsPrefix
        /// </summary>
        public bool IsPrefix { get; }

        /// <summary>
        /// Gets the RouteTemplate
        /// </summary>
        public string RouteTemplate { get; }

        #endregion 属性
    }
}