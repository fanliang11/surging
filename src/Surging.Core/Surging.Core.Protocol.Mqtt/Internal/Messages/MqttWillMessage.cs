using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Messages
{
    /// <summary>
    /// Defines the <see cref="MqttWillMessage" />
    /// </summary>
    public class MqttWillMessage
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Qos
        /// </summary>
        public int Qos { get; set; }

        /// <summary>
        /// Gets or sets the Topic
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Gets or sets the WillMessage
        /// </summary>
        public string WillMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether WillRetain
        /// </summary>
        public bool WillRetain { get; set; }

        #endregion 属性
    }
}