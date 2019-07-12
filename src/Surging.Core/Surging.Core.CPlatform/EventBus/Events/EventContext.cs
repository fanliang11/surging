using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.EventBus.Events
{
    /// <summary>
    /// Defines the <see cref="EventContext" />
    /// </summary>
    public class EventContext : IntegrationEvent
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Content
        /// </summary>
        public object Content { get; set; }

        /// <summary>
        /// Gets or sets the Count
        /// </summary>
        public long Count { get; set; }

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public string Type { get; set; }

        #endregion 属性
    }
}