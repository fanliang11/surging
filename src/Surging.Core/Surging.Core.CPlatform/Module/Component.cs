using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Module
{
    #region 枚举

    /// <summary>
    /// Defines the LifetimeScope
    /// </summary>
    public enum LifetimeScope
    {
        /// <summary>
        /// Defines the InstancePerDependency
        /// </summary>
        InstancePerDependency,

        /// <summary>
        /// Defines the InstancePerHttpRequest
        /// </summary>
        InstancePerHttpRequest,

        /// <summary>
        /// Defines the SingleInstance
        /// </summary>
        SingleInstance
    }

    #endregion 枚举

    /// <summary>
    /// Defines the <see cref="Component" />
    /// </summary>
    public class Component
    {
        #region 属性

        /// <summary>
        /// Gets or sets the ImplementType
        /// </summary>
        public string ImplementType { get; set; }

        /// <summary>
        /// Gets or sets the LifetimeScope
        /// </summary>
        public LifetimeScope LifetimeScope { get; set; }

        /// <summary>
        /// Gets or sets the ServiceType
        /// </summary>
        public string ServiceType { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("接口类型：{0}", ServiceType);
            sb.AppendLine();
            sb.AppendFormat("实现类型：{0}", ImplementType);
            sb.AppendLine();
            sb.AppendFormat("生命周期：{0}", LifetimeScope);
            return sb.ToString();
        }

        #endregion 方法
    }
}