using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.WS.Configurations
{
    public class WebSocketOptions
    {
        /// <summary>
        /// Gets or sets the time to wait for the response to the WebSocket Ping or
        /// Close.
        /// </summary>
        /// <remarks>
        /// The set operation does nothing if the server has already started or
        /// it is shutting down.
        /// </remarks>
        /// <value>
        ///   <para>
        ///   A <see cref="TimeSpan"/> to wait for the response.
        ///   </para>
        ///   <para>
        ///   The default value is the same as 1 second.
        ///   </para>
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The value specified for a set operation is zero or less.
        /// </exception>
        public int WaitTime { get; set; } = 1;
        
        public bool KeepClean
        {
            get;
            set;
        } = false;

        public BehaviorOption Behavior { get; set; }
    }

}