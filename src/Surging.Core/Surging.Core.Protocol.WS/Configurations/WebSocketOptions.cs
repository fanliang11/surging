using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.WS.Configurations
{
    /// <summary>
    /// Defines the <see cref="WebSocketOptions" />
    /// </summary>
    public class WebSocketOptions
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Behavior
        /// </summary>
        public BehaviorOption Behavior { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether KeepClean
        /// </summary>
        public bool KeepClean { get; set; } = false;

        /// <summary>
        /// Gets or sets the time to wait for the response to the WebSocket Ping or
        /// Close.
        /// </summary>
        public int WaitTime { get; set; } = 1;

        #endregion 属性
    }
}