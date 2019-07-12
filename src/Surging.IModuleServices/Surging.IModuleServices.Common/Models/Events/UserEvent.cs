using Surging.Core.CPlatform.EventBus.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.IModuleServices.Common.Models.Events
{
    /// <summary>
    /// Defines the <see cref="UserEvent" />
    /// </summary>
    public class UserEvent : IntegrationEvent
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Age
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the UserId
        /// </summary>
        public int UserId { get; set; }

        #endregion 属性
    }
}