using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.WS.Configurations
{
    /// <summary>
    /// Defines the <see cref="BehaviorOption" />
    /// </summary>
    public class BehaviorOption
    {
        #region 属性

        /// <summary>
        /// Gets or sets a value indicating whether EmitOnPing
        /// </summary>
        public bool EmitOnPing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IgnoreExtensions
        /// </summary>
        public bool IgnoreExtensions { get; set; }

        /// <summary>
        /// Gets or sets the Protocol
        /// </summary>
        public string Protocol { get; set; }

        #endregion 属性
    }
}