using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.DNS.Configurations
{
    /// <summary>
    /// Defines the <see cref="DnsOption" />
    /// </summary>
    public class DnsOption
    {
        #region 属性

        /// <summary>
        /// Gets or sets the QueryTimeout
        /// </summary>
        public int QueryTimeout { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the RootDnsAddress
        /// </summary>
        public string RootDnsAddress { get; set; }

        #endregion 属性
    }
}