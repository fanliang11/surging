using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusKafka.Implementation
{
    #region 枚举

    /// <summary>
    /// Defines the KafkaConnectionType
    /// </summary>
    public enum KafkaConnectionType
    {
        /// <summary>
        /// Defines the Producer
        /// </summary>
        Producer,

        /// <summary>
        /// Defines the Consumer
        /// </summary>
        Consumer
    }

    #endregion 枚举
}