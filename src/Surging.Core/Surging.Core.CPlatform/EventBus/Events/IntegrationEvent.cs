using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.EventBus.Events
{
    /// <summary>
    /// Defines the <see cref="IntegrationEvent" />
    /// </summary>
    public class IntegrationEvent
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationEvent"/> class.
        /// </summary>
        public IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationEvent"/> class.
        /// </summary>
        /// <param name="integrationEvent">The integrationEvent<see cref="IntegrationEvent"/></param>
        public IntegrationEvent(IntegrationEvent integrationEvent)
        {
            Id = integrationEvent.Id;
            CreationDate = integrationEvent.CreationDate;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the CreationDate
        /// </summary>
        public DateTime CreationDate { get; }

        /// <summary>
        /// Gets the Id
        /// </summary>
        public Guid Id { get; }

        #endregion 属性
    }
}