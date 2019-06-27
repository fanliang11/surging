using Surging.Core.CPlatform.Address;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Surging.Core.Consul.Configurations
{
    public class ConfigInfo
    {
        /// <summary>
        /// 初始化会话超时为20秒的consul配置信息。
        /// </summary>
        /// <param name="connectionString">连接字符串。</param>
        /// <param name="commandPath">服务命令配置路径</param>
        /// <param name="routePath">路由路径配置路径</param>
        /// <param name="subscriberPath">订阅者配置路径</param>
        /// <param name="cachePath">缓存中心配置路径</param>
        public ConfigInfo(string connectionString,string routePath = "services/serviceRoutes/",
             string subscriberPath = "services/serviceSubscribers/",
            string commandPath = "services/serviceCommands/",
            string cachePath="services/serviceCaches/",
            string mqttRoutePath = "services/mqttServiceRoutes/",
            bool reloadOnChange=false, bool enableChildrenMonitor = false) :
            this(connectionString, TimeSpan.FromSeconds(20), 0, routePath, subscriberPath,commandPath, cachePath, mqttRoutePath, reloadOnChange, enableChildrenMonitor)
        {
        }

        /// <summary>
        /// 初始化consul配置信息。
        /// </summary>
        /// <param name="connectionString">连接字符串。</param>
        /// <param name="sessionTimeout">会话超时时间。</param>
        /// <param name="commandPath">服务命令配置命令。</param>
        /// <param name="subscriberPath">订阅者配置命令。</param>
        /// <param name="routePath">路由路径配置路径</param>
        /// <param name="cachePath">缓存中心配置路径</param>
        /// <param name="mqttRoutePath">Mqtt路由路径配置路径</param>
        public ConfigInfo(string connectionString, TimeSpan sessionTimeout, int lockDelay,
            string routePath = "services/serviceRoutes/",
             string subscriberPath = "services/serviceSubscribers/",
            string commandPath = "services/serviceCommands/",
            string cachePath= "services/serviceCaches/",
            string mqttRoutePath= "services/mqttServiceRoutes/",
            bool reloadOnChange=false, bool enableChildrenMonitor = false)
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
                    if (address !=null)
                    { 
                        var ipAddress=address as IpAddressModel;
                        Host = ipAddress.Ip;
                        Port = ipAddress.Port;
                    }
                    Addresses = new IpAddressModel[] {
                        new IpAddressModel(Host,Port)
                    };
                }
            }
        }

        public ConfigInfo(string host, int port) : this(host, port, TimeSpan.FromSeconds(20))
        {
        }

        public ConfigInfo(string host, int port, TimeSpan sessionTimeout)
        {
            SessionTimeout = sessionTimeout;
            Host = host;
            Port = port;
        }

        public bool ReloadOnChange { get; set; }

        /// <summary>
        /// watch 时间间隔
        /// </summary>
        public int WatchInterval { get; set; } = 60;

        public int LockDelay { get; set; }

        public bool EnableChildrenMonitor { get; set; }
        /// <summary>
        /// 命令配置路径
        /// </summary>
        public string CommandPath { get; set; }

        /// <summary>
        /// 订阅者配置路径
        /// </summary>
        public string SubscriberPath { get; set; }

        /// <summary>
        /// 路由配置路径。
        /// </summary>
        public string RoutePath { get; set; }


        /// <summary>
        /// Mqtt路由配置路径。
        /// </summary>
        public string MqttRoutePath { get; set; }

        public IEnumerable<AddressModel> Addresses { get; set; }

        /// <summary>
        /// 缓存中心配置中心
        /// </summary>
        public string CachePath { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        /// <summary>
        /// 会话超时时间。
        /// </summary>
        public TimeSpan SessionTimeout { get; set; }

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
