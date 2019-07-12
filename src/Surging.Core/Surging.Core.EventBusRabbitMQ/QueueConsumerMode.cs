using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusRabbitMQ
{
    #region 枚举

    /// <summary>
    /// Defines the QueueConsumerMode
    /// </summary>
    public enum QueueConsumerMode
    {
        /// <summary>
        /// Defines the Normal
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Defines the Retry
        /// </summary>
        Retry,

        /// <summary>
        /// Defines the Fail
        /// </summary>
        Fail,
    }

    #endregion 枚举
}