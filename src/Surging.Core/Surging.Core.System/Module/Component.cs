using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.System.Module
{
    #region 枚举

    /// <summary>
    /// 组件生命周期枚举。
    /// </summary>
    /// <remarks>
    /// 	<para>创建：范亮</para>
    /// 	<para>日期：2015/12/4</para>
    /// </remarks>
    public enum LifetimeScope
    {
        /// <summary>
        /// 每次依赖的时候实例化对象。
        /// </summary>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/4</para>
        /// </remarks>
        InstancePerDependency,

        /// <summary>
        /// 每次 Http Request 请求的时候实例化对象。
        /// </summary>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/4</para>
        /// </remarks>
        InstancePerHttpRequest,

        /// <summary>
        /// 单实例化对象。
        /// </summary>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2015/12/4</para>
        /// </remarks>
        SingleInstance
    }

    #endregion 枚举

    /// <summary>
    /// 组件描述类(定义了接口+实现类)。
    /// </summary>
    public class Component
    {
        #region 属性

        /// <summary>
        /// Gets or sets the ImplementType
        /// 获取或设置接口实现类的类型名称(包含程序集名称的限定名)。
        /// </summary>
        public string ImplementType { get; set; }

        /// <summary>
        /// Gets or sets the LifetimeScope
        /// 获取或设置组件生命周期枚举。
        /// </summary>
        public LifetimeScope LifetimeScope { get; set; }

        /// <summary>
        /// Gets or sets the ServiceType
        /// 获取或设置接口服务类型名称(包含程序集名称的限定名)。
        /// </summary>
        public string ServiceType { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// 获取组件的字符串文本描述信息。
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