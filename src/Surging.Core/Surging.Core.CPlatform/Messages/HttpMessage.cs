using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Messages
{
    /// <summary>
    /// Defines the <see cref="HttpMessage" />
    /// </summary>
    public class HttpMessage
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Parameters
        /// </summary>
        public IDictionary<string, object> Parameters { get; set; }

        /// <summary>
        /// Gets or sets the RoutePath
        /// </summary>
        public string RoutePath { get; set; }

        /// <summary>
        /// Gets or sets the ServiceKey
        /// </summary>
        public string ServiceKey { get; set; }

        #endregion 属性
    }
}