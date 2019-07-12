using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
    #region 枚举

    /// <summary>
    /// Defines the MessageType
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Defines the CONNECT
        /// </summary>
        CONNECT = 1,

        /// <summary>
        /// Defines the CONNACK
        /// </summary>
        CONNACK = 2,

        /// <summary>
        /// Defines the PUBLISH
        /// </summary>
        PUBLISH = 3,

        /// <summary>
        /// Defines the PUBACK
        /// </summary>
        PUBACK = 4,

        /// <summary>
        /// Defines the PUBREC
        /// </summary>
        PUBREC = 5,

        /// <summary>
        /// Defines the PUBREL
        /// </summary>
        PUBREL = 6,

        /// <summary>
        /// Defines the PUBCOMP
        /// </summary>
        PUBCOMP = 7,

        /// <summary>
        /// Defines the SUBSCRIBE
        /// </summary>
        SUBSCRIBE = 8,

        /// <summary>
        /// Defines the SUBACK
        /// </summary>
        SUBACK = 9,

        /// <summary>
        /// Defines the UNSUBSCRIBE
        /// </summary>
        UNSUBSCRIBE = 10,

        /// <summary>
        /// Defines the UNSUBACK
        /// </summary>
        UNSUBACK = 11,

        /// <summary>
        /// Defines the PINGREQ
        /// </summary>
        PINGREQ = 12,

        /// <summary>
        /// Defines the PINGRESP
        /// </summary>
        PINGRESP = 13,

        /// <summary>
        /// Defines the DISCONNECT
        /// </summary>
        DISCONNECT = 14
    }

    #endregion 枚举
}