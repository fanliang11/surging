using Surging.Core.CPlatform.Address;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace Surging.Core.CPlatform.Utilities
{
    /// <summary>
    /// Defines the <see cref="NetUtils" />
    /// </summary>
    public class NetUtils
    {
        #region 常量

        /// <summary>
        /// Defines the ANYHOST
        /// </summary>
        public const string ANYHOST = "0.0.0.0";

        /// <summary>
        /// Defines the LOCALHOST
        /// </summary>
        public const string LOCALHOST = "127.0.0.1";

        /// <summary>
        /// Defines the IP_PATTERN
        /// </summary>
        private const string IP_PATTERN = "\\d{1,3}(\\.\\d{1,3}){3,5}$";

        /// <summary>
        /// Defines the LOCAL_IP_PATTERN
        /// </summary>
        private const string LOCAL_IP_PATTERN = "127(\\.\\d{1,3}){3}$";

        /// <summary>
        /// Defines the MAX_PORT
        /// </summary>
        private const int MAX_PORT = 65535;

        /// <summary>
        /// Defines the MIN_PORT
        /// </summary>
        private const int MIN_PORT = 0;

        #endregion 常量

        #region 字段

        /// <summary>
        /// Defines the _host
        /// </summary>
        private static AddressModel _host = null;

        #endregion 字段

        #region 方法

        /// <summary>
        /// The GetAnyHostAddress
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public static string GetAnyHostAddress()
        {
            string result = "";
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    IPInterfaceProperties ipxx = adapter.GetIPProperties();
                    UnicastIPAddressInformationCollection ipCollection = ipxx.UnicastAddresses;
                    foreach (UnicastIPAddressInformation ipadd in ipCollection)
                    {
                        if (ipadd.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            result = ipadd.Address.ToString();
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// The GetHostAddress
        /// </summary>
        /// <returns>The <see cref="AddressModel"/></returns>
        public static AddressModel GetHostAddress()
        {
            if (_host != null)
                return _host;
            var ports = AppConfig.ServerOptions.Ports;
            string address = GetHostAddress(AppConfig.ServerOptions.Ip);
            int port = AppConfig.ServerOptions.Port;
            var mappingIp = AppConfig.ServerOptions.MappingIP ?? address;
            var mappingPort = AppConfig.ServerOptions.MappingPort;
            if (mappingPort == 0)
                mappingPort = port;
            _host = new IpAddressModel
            {
                HttpPort = ports.HttpPort,
                Ip = mappingIp,
                Port = mappingPort,
                MqttPort = ports.MQTTPort,
                WanIp = AppConfig.ServerOptions.WanIp,
                WsPort = ports.WSPort
            };
            return _host;
        }

        /// <summary>
        /// The GetHostAddress
        /// </summary>
        /// <param name="hostAddress">The hostAddress<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string GetHostAddress(string hostAddress)
        {
            var result = hostAddress;
            if ((!IsValidAddress(hostAddress) && !IsLocalHost(hostAddress)) || IsAnyHost(hostAddress))
            {
                result = GetAnyHostAddress();
            }
            return result;
        }

        /// <summary>
        /// The IsAnyHost
        /// </summary>
        /// <param name="host">The host<see cref="String"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static bool IsAnyHost(String host)
        {
            return "0.0.0.0".Equals(host);
        }

        /// <summary>
        /// The IsInvalidLocalHost
        /// </summary>
        /// <param name="host">The host<see cref="String"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static bool IsInvalidLocalHost(String host)
        {
            return host == null
                    || host.Length == 0
                    || host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                    || host.Equals("0.0.0.0")
                    || (host.IsMatch(LOCAL_IP_PATTERN));
        }

        /// <summary>
        /// The IsInvalidPort
        /// </summary>
        /// <param name="port">The port<see cref="int"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static bool IsInvalidPort(int port)
        {
            return port <= MIN_PORT || port > MAX_PORT;
        }

        /// <summary>
        /// The IsLocalHost
        /// </summary>
        /// <param name="host">The host<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public static bool IsLocalHost(string host)
        {
            return host != null
                    && (host.IsMatch(LOCAL_IP_PATTERN)
                    || host.Equals("localhost", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// The IsValidAddress
        /// </summary>
        /// <param name="address">The address<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
        private static bool IsValidAddress(string address)
        {
            return (address != null
                    && !ANYHOST.Equals(address)
                    && address.IsMatch(IP_PATTERN));
        }

        #endregion 方法
    }
}