using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.EventBusKafka.Configurations
{
    #region 枚举

    /// <summary>
    /// Defines the OffsetResetMode
    /// </summary>
    public enum OffsetResetMode
    {
        /// <summary>
        /// Defines the Earliest
        /// </summary>
        Earliest,

        /// <summary>
        /// Defines the Latest
        /// </summary>
        Latest,

        /// <summary>
        /// Defines the None
        /// </summary>
        None,
    }

    #endregion 枚举
}