using Surging.Core.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Filters.Implementation
{
    /// <summary>
    /// Defines the <see cref="RpcActionExecutedContext" />
    /// </summary>
    public class RpcActionExecutedContext
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Exception
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets the InvokeMessage
        /// </summary>
        public RemoteInvokeMessage InvokeMessage { get; set; }

        #endregion 属性
    }
}