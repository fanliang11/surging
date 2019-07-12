using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Routing.Implementation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Routing
{
    /// <summary>
    /// Defines the <see cref="ServiceRouteContext" />
    /// </summary>
    public class ServiceRouteContext
    {
        #region 属性

        /// <summary>
        /// Gets or sets the InvokeMessage
        /// </summary>
        public RemoteInvokeMessage InvokeMessage { get; set; }

        /// <summary>
        /// Gets or sets the ResultMessage
        /// </summary>
        public RemoteInvokeResultMessage ResultMessage { get; set; }

        /// <summary>
        /// Gets or sets the Route
        /// </summary>
        public ServiceRoute Route { get; set; }

        #endregion 属性
    }
}