using Surging.Core.CPlatform.Address;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surging.Core.Zookeeper.Configurations
{
    public class ConfigInfo
    {
        /// <summary>
        /// 初始化会话超时为20秒的Zookeeper配置信息。
        /// </summary>
        /// <param name="connectionString">连接字符串。</param>
        /// <param name="routePath">路由配置路径。</param>
        /// <param name="subscriberPath">订阅者配置路径</param>
        /// <param name="commandPath">服务命令配置路径</param>
        /// <param name="cachePath">缓存中心配置路径</param>
        /// <param name="mqttRoutePath">mqtt路由配置路径</param>
        /// <param name="chRoot">根节点。</param>
        public ConfigInfo(string connectionString, string routePath = "/services/serviceRoutes",
            string subscriberPath = "/services/serviceSubscribers",
            string commandPath = "/services/serviceCommands",
            string cachePath = "/services/serviceCaches",
            string mqttRoutePath = "/services/mqttServiceRoutes",
            string chRoot = null,
            bool reloadOnChange = false, bool enableChildrenMonitor = false) : this(connectionString,
                TimeSpan.FromSeconds(20),
                routePath,
                subscriberPath,
                commandPath,
                cachePath,
                mqttRoutePath,
                chRoot,
                reloadOnChange, enableChildrenMonitor)
        {
        }

        /// <summary>
        /// 初始化Zookeeper配置信息。
        /// </summary>
        /// <param name="connectionString">连接字符串。</param>
        /// <param name="routePath">路由配置路径。</param>
        /// <param name="commandPath">服务命令配置路径</param>
        /// <param name="subscriberPath">订阅者配置路径</param>
        /// <param name="sessionTimeout">会话超时时间。</param>
        /// <param name="cachePath">缓存中心配置路径</param>
        /// <param name="mqttRoutePath">mqtt路由配置路径</param>
        /// <param name="chRoot">根节点。</param>
        public ConfigInfo(string connectionString, TimeSpan sessionTimeout, string routePath = "/services/serviceRoutes",
            string subscriberPath = "/services/serviceSubscribers",
            string commandPath = "/services/serviceCommands",
            string cachePath = "/services/serviceCaches",
            string mqttRoutePath = "/services/mqttServiceRoutes",
            string chRoot = null,
            bool reloadOnChange = false, bool enableChildrenMonitor = false)
        {
            CachePath = cachePath;
            ReloadOnChange = reloadOnChange;
            ChRoot = chRoot;
            CommandPath = commandPath;
            SubscriberPath = subscriberPath;
            ConnectionString = connectionString;
            RoutePath = routePath;
            SessionTimeout = sessionTimeout;
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
                        Addresses = new IpAddressModel[] { ipAddress};
                    }
                }
            }
        }

        public bool EnableChildrenMonitor { get; set; }

        public bool ReloadOnChange { get; set; }

        /// <summary>
        /// 连接字符串。
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 命令配置路径
        /// </summary>
        public string CommandPath { get; set; }

        /// <summary>
        /// 路由配置路径。
        /// </summary>
        public string RoutePath { get; set; }

        /// <summary>
        /// 订阅者配置路径
        /// </summary>
        public string SubscriberPath { get; set; }

        /// <summary>
        /// 会话超时时间。
        /// </summary>
        public TimeSpan SessionTimeout { get; set; }

        /// <summary>
        /// 根节点。
        /// </summary>
        public string ChRoot { get; set; }


        public IEnumerable<AddressModel> Addresses { get; set; }

        /// <summary>
        /// 缓存中心配置中心
        /// </summary>
        public string CachePath { get; set; }


        /// <summary>
        /// Mqtt路由配置路径。
        /// </summary>
        public string MqttRoutePath { get; set; }

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
    }
}
