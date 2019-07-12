using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surging.Core.EventBusRabbitMQ.Attributes
{
    /// <summary>
    /// Defines the <see cref="QueueConsumerAttribute" />
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class QueueConsumerAttribute : Attribute
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueConsumerAttribute"/> class.
        /// </summary>
        /// <param name="queueName">The queueName<see cref="string"/></param>
        /// <param name="modes">The modes<see cref="QueueConsumerMode[]"/></param>
        public QueueConsumerAttribute(string queueName, params QueueConsumerMode[] modes)
        {
            _queueName = queueName;
            _modes = modes.Any() ? modes :
                new QueueConsumerMode[] { QueueConsumerMode.Normal };
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the Modes
        /// </summary>
        public QueueConsumerMode[] Modes
        {
            get { return _modes; }
        }

        /// <summary>
        /// Gets the QueueName
        /// </summary>
        public string QueueName
        {
            get { return _queueName; }
        }

        /// <summary>
        /// Gets or sets the _modes
        /// </summary>
        private QueueConsumerMode[] _modes { get; set; }

        /// <summary>
        /// Gets or sets the _queueName
        /// </summary>
        private string _queueName { get; set; }

        #endregion 属性
    }
}