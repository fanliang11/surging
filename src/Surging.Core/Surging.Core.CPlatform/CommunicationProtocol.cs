using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform
{
    #region 枚举

    /// <summary>
    /// Defines the CommunicationProtocol
    /// </summary>
    public enum CommunicationProtocol
    {
        /// <summary>
        /// Defines the None
        /// </summary>
        None,

        /// <summary>
        /// Defines the Tcp
        /// </summary>
        Tcp,

        /// <summary>
        /// Defines the Http
        /// </summary>
        Http,

        /// <summary>
        /// Defines the WS
        /// </summary>
        WS,

        /// <summary>
        /// Defines the Mqtt
        /// </summary>
        Mqtt,

        /// <summary>
        /// Defines the Dns
        /// </summary>
        Dns
    }

    #endregion 枚举
}