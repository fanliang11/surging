using Surging.Core.CPlatform.Address;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Surging.Core.Consul.Configurations
{
    /// <summary>
    /// Defines the <see cref="ConfigInfo" />
    /// </summary>
    public class ConfigInfo
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigInfo"/> class.
        /// </summary>
        /// <param name="connectionString">连接字符串。</param>
        /// <param name="sessionTimeout">会话超时时间。</param>
        /// <param name="lockDelay">The lockDelay<see cref="int"/></param>
        /// <param name="routePath">路由路径配置路径</param>
        /// <param name="subscriberPath">订阅者配置命令。</param>
        /// <param name="commandPath">服务命令配置命令。</param>
        /// <param name="cachePath">缓存中心配置路径</param>
        /// <param name="mqttRoutePath">Mqtt路由路径配置路径</param>
        /// <param name="reloadOnChange">The reloadOnChange<see cref="bool"/></param>
        /// <param name="enableChildrenMonitor">The enableChildrenMonitor<see cref="bool"/></param>
        public ConfigInfo(string connectionString, TimeSpan sessionTimeout, int lockDelay,
            string routePath = "services/serviceRoutes/",
             string subscriberPath = "services/serviceSubscribers/",
            string commandPath = "services/serviceCommands/",
            string cachePath = "services/serviceCaches/",
            string mqttRoutePath = "services/mqttServiceRoutes/",
            bool reloadOnChange = false, bool enableChildrenMonitor = false)
        {
            CachePath = cachePath;
            ReloadOnChange = reloadOnChange;
            SessionTimeout = sessionTimeout;
            RoutePath = routePath;
            LockDelay = lockDelay;
            SubscriberPath = subscriberPath;
            CommandPath = commandPath;
            MqttRoutePath = mqttRoutePath;
            EnableChildrenMonitor = enableChildrenMonitor;
            if (!string.IsNullOrEmpty(connectionString))
            {
                var addresses = connectionString.Split(",");
                if (addresses.Length > 1)
                {
                    Addresses = addresses.Select(p => ConvertAddressModel(p));
                }
                else
                {
                    var address = ConvertAddressModel(connectionString);
                    if (address != null)
                    {
                        var ipAddress = address as IpAddressModel;
                        Host = ipAddress.Ip;
                        Port = ipAddress.Port;
                    }
                    Addresses = new IpAddressModel[] {
                        new IpAddressModel(Host,Port)
                    };
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigInfo"/> class.
        /// </summary>
        /// <param name="connectionString">连接字符串。</param>
        /// <param name="routePath">路由路径配置路径</param>
        /// <param name="subscriberPath">订阅者配置路径</param>
        /// <param name="commandPath">服务命令配置路径</param>
        /// <param name="cachePath">缓存中心配置路径</param>
        /// <param name="mqttRoutePath">The mqttRoutePath<see cref="string"/></param>
        /// <param name="reloadOnChange">The reloadOnChange<see cref="bool"/></param>
        /// <param name="enableChildrenMonitor">The enableChildrenMonitor<see cref="bool"/></param>
        public ConfigInfo(string connectionString, string routePath = "services/serviceRoutes/",
             string subscriberPath = "services/serviceSubscribers/",
            string commandPath = "services/serviceCommands/",
            string cachePath = "services/serviceCaches/",
            string mqttRoutePath = "services/mqttServiceRoutes/",
            bool reloadOnChange = false, bool enableChildrenMonitor = false) :
            this(connectionString, TimeSpan.FromSeconds(20), 0, routePath, subscriberPath, commandPath, cachePath, mqttRoutePath, reloadOnChange, enableChildrenMonitor)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigInfo"/> class.
        /// </summary>
        /// <param name="host">The host<see cref="string"/></param>
        /// <param name="port">The port<see cref="int"/></param>
        public ConfigInfo(string host, int port) : this(host, port, TimeSpan.FromSeconds(20))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigInfo"/> class.
        /// </summary>
        /// <param name="host">The host<see cref="string"/></param>
        /// <param name="port">The port<see cref="int"/></param>
        /// <param name="sessionTimeout">The sessionTimeout<see cref="TimeSpan"/></param>
        public ConfigInfo(string host, int port, TimeSpan sessionTimeout)
        {
            SessionTimeout = sessionTimeout;
            Host = host;
            Port = port;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Addresses
        /// </summary>
        public IEnumerable<AddressModel> Addresses { get; set; }

        /// <summary>
        /// Gets or sets the CachePath
        /// 缓存中心配置中心
        /// </summary>
        public string CachePath { get; set; }

        /// <summary>
        /// Gets or sets the CommandPath
        /// 命令配置路径
        /// </summary>
        public string CommandPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether EnableChildrenMonitor
        /// </summary>
        public bool EnableChildrenMonitor { get; set; }

        /// <summary>
        /// Gets or sets the Host
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the LockDelay
        /// </summary>
        public int LockDelay { get; set; }

        /// <summary>
        /// Gets or sets the MqttRoutePath
        /// Mqtt路由配置路径。
        /// </summary>
        public string MqttRoutePath { get; set; }

        /// <summary>
        /// Gets or sets the Port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether ReloadOnChange
        /// </summary>
        public bool ReloadOnChange { get; set; }

        /// <summary>
        /// Gets or sets the RoutePath
        /// 路由配置路径。
        /// </summary>
        public string RoutePath { get; set; }

        /// <summary>
        /// Gets or sets the SessionTimeout
        /// 会话超时时间。
        /// </summary>
        public TimeSpan SessionTimeout { get; set; }

        /// <summary>
        /// Gets or sets the SubscriberPath
        /// 订阅者配置路径
        /// </summary>
        public string SubscriberPath { get; set; }

        /// <summary>
        /// Gets or sets the WatchInterval
        /// watch 时间间隔
        /// </summary>
        public int WatchInterval { get; set; } = 60;

        #endregion 属性

        #region 方法

        /// <summary>
        /// The ConvertAddressModel
        /// </summary>
        /// <param name="connection">The connection<see cref="string"/></param>
        /// <returns>The <see cref="AddressModel"/></returns>
        public AddressModel ConvertAddressModel(string connection)
        {
            var address = connection.Split(":");
            if (address.Length > 1)
            {
                int port;
                int.TryParse(address[1], out port);
                return new IpAddressModel(address[0], port);
            }
            return null;
        }

        #endregion 方法
    }
}