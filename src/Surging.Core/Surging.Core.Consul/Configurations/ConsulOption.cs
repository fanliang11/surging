using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Consul.Configurations
{
    /// <summary>
    /// Consul选项 <see cref="ConsulOption" />
    /// </summary>
    public class ConsulOption
    {
        #region 属性

        /// <summary>
        /// Gets or sets the CachePath
        /// 缓存路径
        /// </summary>
        public string CachePath { get; set; }

        /// <summary>
        /// Gets or sets the CommandPath
        /// 指令路径
        /// </summary>
        public string CommandPath { get; set; }

        /// <summary>
        /// Gets or sets the ConnectionString
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the EnableChildrenMonitor
        /// 是否启用子孙配置
        /// </summary>
        public string EnableChildrenMonitor { get; set; }

        /// <summary>
        /// Gets or sets the LockDelay
        /// 锁定延时
        /// </summary>
        public int? LockDelay { get; set; }

        /// <summary>
        /// Gets or sets the MqttRoutePath
        /// mqtt路由路径
        /// </summary>
        public string MqttRoutePath { get; set; }

        /// <summary>
        /// Gets or sets the ReloadOnChange
        /// 改变时重加载
        /// </summary>
        public string ReloadOnChange { get; set; }

        /// <summary>
        /// Gets or sets the RoutePath
        /// 路由路径
        /// </summary>
        public string RoutePath { get; set; }

        /// <summary>
        /// Gets or sets the SessionTimeout
        /// Session过期间隔
        /// </summary>
        public string SessionTimeout { get; set; }

        /// <summary>
        /// Gets or sets the SubscriberPath
        /// 订阅路径
        /// </summary>
        public string SubscriberPath { get; set; }

        #endregion 属性
    }
}