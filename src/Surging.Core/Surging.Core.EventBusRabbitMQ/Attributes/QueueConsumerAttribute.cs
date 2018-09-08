using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surging.Core.EventBusRabbitMQ.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class QueueConsumerAttribute : Attribute
    {
        public string QueueName
        {
            get { return _queueName; }
        }

        public QueueConsumerMode[] Modes
        {
            get { return _modes; }
        }

        private string _queueName { get; set; }

        private QueueConsumerMode[] _modes { get; set; }

        public QueueConsumerAttribute(string queueName, params QueueConsumerMode[] modes)
        {
            _queueName = queueName;
            _modes = modes.Any() ? modes :
                new QueueConsumerMode[] { QueueConsumerMode.Normal };
        }

    }
}
