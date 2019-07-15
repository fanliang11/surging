using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Surging.Core.CPlatform.Address
{
    /// <summary>
    /// 地址操作者 <see cref="AddressHelper" />
    /// </summary>
    public class AddressHelper
    {
        #region 方法

        /// <summary>
        /// 根据地址获取IP
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        /// <exception cref="System.Net.Sockets.SocketException"></exception>
        /// <exception cref="RegexMatchTimeoutException"></exception>
        public static string GetIpFromAddress(string address)
        {
            if (IsValidIp(address))
            {
                return address;
            }
            var ips = Dns.GetHostAddresses(address);
            var ipRes = ips[0].ToString();
            foreach (var itemIP in ips)
            {
                if (IsValidIp(itemIP.ToString()))
                {
                    ipRes = itemIP.ToString();
                    break;
                }
            }
            return ipRes;
        }

        /// <summary>
        /// 判断地址是不是IP
        /// </summary>
        /// <param name="address">地址</param>
        /// <returns></returns>
        /// <exception cref="RegexMatchTimeoutException"></exception>
        public static bool IsValidIp(string address)
        {
            if (Regex.IsMatch(address, "[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}"))
            {
                string[] ips = address.Split('.');
                if (ips.Length == 4 || ips.Length == 6)
                {
                    if (int.Parse(ips[0]) < 256 && int.Parse(ips[1]) < 256 && int.Parse(ips[2]) < 256 && int.Parse(ips[3]) < 256)
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }

        #endregion 方法
    }
}