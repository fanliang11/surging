using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Messages
{
    /// <summary>
    /// Defines the <see cref="RetainMessage" />
    /// </summary>
    public class RetainMessage
    {
        #region 属性

        /// <summary>
        /// Gets or sets the ByteBuf
        /// </summary>
        public byte[] ByteBuf { get; set; }

        /// <summary>
        /// Gets or sets the QoS
        /// </summary>
        public int QoS { get; set; }

        /// <summary>
        /// Gets the ToString
        /// </summary>
        public new string ToString => Encoding.UTF8.GetString(ByteBuf);

        #endregion 属性
    }
}