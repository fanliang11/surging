using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.DNS.Runtime
{
    /// <summary>
    /// Defines the <see cref="DnsServiceEntry" />
    /// </summary>
    public class DnsServiceEntry
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Behavior
        /// </summary>
        public DnsBehavior Behavior { get; set; }

        /// <summary>
        /// Gets or sets the Path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public Type Type { get; set; }

        #endregion 属性
    }
}