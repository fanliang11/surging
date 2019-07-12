using Surging.Core.ApiGateWay.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay
{
    /// <summary>
    /// Defines the <see cref="Register" />
    /// </summary>
    public class Register
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Address
        /// </summary>
        public string Address { get; set; } = "127.0.0.1:8500";

        /// <summary>
        /// Gets or sets the Provider
        /// </summary>
        public RegisterProvider Provider { get; set; } = RegisterProvider.Consul;

        #endregion 属性
    }
}