using Surging.Core.CPlatform.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.IModuleServices.Common.Models.Events
{
    /// <summary>
    /// Defines the <see cref="LogoutEvent" />
    /// </summary>
    public class LogoutEvent : IntegrationEvent
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Age
        /// </summary>
        public string Age { get; set; }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the UserId
        /// </summary>
        public string UserId { get; set; }

        #endregion 属性
    }
}