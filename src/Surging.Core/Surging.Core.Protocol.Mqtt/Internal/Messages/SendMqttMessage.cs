using DotNetty.Transport.Channels;
using Surging.Core.Protocol.Mqtt.Internal.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Messages
{
    /// <summary>
    /// Defines the <see cref="SendMqttMessage" />
    /// </summary>
    public class SendMqttMessage
    {
        #region 属性

        /// <summary>
        /// Gets or sets the ByteBuf
        /// </summary>
        public byte[] ByteBuf { get; set; }

        /// <summary>
        /// Gets or sets the Channel
        /// </summary>
        public IChannel Channel { get; set; }

        /// <summary>
        /// Gets or sets the ConfirmStatus
        /// </summary>
        public ConfirmStatus ConfirmStatus { get; set; }

        /// <summary>
        /// Gets or sets the MessageId
        /// </summary>
        public int MessageId { get; set; }

        /// <summary>
        /// Gets or sets the Qos
        /// </summary>
        public int Qos { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Retain
        /// </summary>
        public bool Retain { get; set; }

        /// <summary>
        /// Gets or sets the Time
        /// </summary>
        public long Time { get; set; }

        /// <summary>
        /// Gets or sets the Topic
        /// </summary>
        public string Topic { get; set; }

        #endregion 属性
    }
}