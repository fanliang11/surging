using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Surging.Core.CPlatform.Address
{
    /// <summary>
    /// Defines the <see cref="AddressHelper" />
    /// </summary>
    public class AddressHelper
    {
        #region 方法

        /// <summary>
        /// The GetIpFromAddress
        /// </summary>
        /// <param name="address">The address<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string GetIpFromAddress(string address)
        {
            if (IsValidIp(address))
            {
                return address;
            }
            var ips = Dns.GetHostAddresses(address);
            return ips[0].ToString();
        }

        /// <summary>
        /// The IsValidIp
        /// </summary>
        /// <param name="address">The address<see cref="string"/></param>
        /// <returns>The <see cref="bool"/></returns>
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