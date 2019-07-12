using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Messages
{
    /// <summary>
    /// Defines the <see cref="SessionMessage" />
    /// </summary>
    public class SessionMessage
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Message
        /// </summary>
        public byte[] Message { get; set; }

        /// <summary>
        /// Gets or sets the QoS
        /// </summary>
        public int QoS { get; set; }

        /// <summary>
        /// Gets or sets the Topic
        /// </summary>
        public string Topic { get; set; }

        #endregion 属性
    }
}