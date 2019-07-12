using Surging.Core.Protocol.WS.Configurations;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocketCore.Server;

namespace Surging.Core.Protocol.WS.Runtime
{
    /// <summary>
    /// Defines the <see cref="WSServiceEntry" />
    /// </summary>
    public class WSServiceEntry
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Behavior
        /// </summary>
        public WebSocketBehavior Behavior { get; set; }

        /// <summary>
        /// Gets or sets the FuncBehavior
        /// </summary>
        public Func<WebSocketBehavior> FuncBehavior { get; set; }

        /// <summary>
        /// Gets or sets the Path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public Type Type { get; set; }

        #endregion 属性
    }
}