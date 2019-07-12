using Microsoft.Extensions.Configuration;
using Surging.Core.DNS.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.DNS
{
    /// <summary>
    /// Defines the <see cref="AppConfig" />
    /// </summary>
    public static class AppConfig
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Configuration
        /// </summary>
        public static IConfigurationRoot Configuration { get; set; }

        /// <summary>
        /// Gets or sets the DnsOption
        /// </summary>
        public static DnsOption DnsOption { get; set; }

        #endregion 属性
    }
}