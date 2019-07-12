using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
    #region 枚举

    /// <summary>
    /// Defines the ConnReturnCode
    /// </summary>
    public enum ConnReturnCode
    {
        /// <summary>
        /// Defines the Accepted
        /// </summary>
        Accepted = 0x00,

        /// <summary>
        /// Defines the RefusedUnacceptableProtocolVersion
        /// </summary>
        RefusedUnacceptableProtocolVersion = 0X01,

        /// <summary>
        /// Defines the RefusedIdentifierRejected
        /// </summary>
        RefusedIdentifierRejected = 0x02,

        /// <summary>
        /// Defines the RefusedServerUnavailable
        /// </summary>
        RefusedServerUnavailable = 0x03,

        /// <summary>
        /// Defines the RefusedBadUsernameOrPassword
        /// </summary>
        RefusedBadUsernameOrPassword = 0x04,

        /// <summary>
        /// Defines the RefusedNotAuthorized
        /// </summary>
        RefusedNotAuthorized = 0x05
    }

    #endregion 枚举
}