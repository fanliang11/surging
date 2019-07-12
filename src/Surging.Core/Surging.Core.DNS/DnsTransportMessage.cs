using DotNetty.Codecs.DNS.Messages;
using DotNetty.Codecs.DNS.Records;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Surging.Core.DNS
{
    /// <summary>
    /// Defines the <see cref="DnsTransportMessage" />
    /// </summary>
    public class DnsTransportMessage
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Address
        /// </summary>
        public IPAddress Address { get; set; }

        /// <summary>
        /// Gets or sets the DnsQuestion
        /// </summary>
        public IDnsQuestion DnsQuestion { get; set; }

        /// <summary>
        /// Gets or sets the DnsResponse
        /// </summary>
        public IDnsResponse DnsResponse { get; set; }

        #endregion 属性
    }
}