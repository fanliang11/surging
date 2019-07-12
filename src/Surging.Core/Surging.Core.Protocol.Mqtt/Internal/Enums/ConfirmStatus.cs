using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
    #region 枚举

    /// <summary>
    /// Defines the ConfirmStatus
    /// </summary>
    public enum ConfirmStatus
    {
        /// <summary>
        /// Defines the PUB
        /// </summary>
        PUB,

        /// <summary>
        /// Defines the PUBREC
        /// </summary>
        PUBREC,

        /// <summary>
        /// Defines the PUBREL
        /// </summary>
        PUBREL,

        /// <summary>
        /// Defines the COMPLETE
        /// </summary>
        COMPLETE,
    }

    #endregion 枚举
}