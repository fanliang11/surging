using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.IModuleServices.Common.Models
{
    /// <summary>
    /// Defines the <see cref="WillMessage" />
    /// </summary>
    public class WillMessage
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the Qos
        /// </summary>
        public int Qos { get; set; }

        /// <summary>
        /// Gets or sets the Topic
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether WillRetain
        /// </summary>
        public bool WillRetain { get; set; }

        #endregion 属性
    }
}