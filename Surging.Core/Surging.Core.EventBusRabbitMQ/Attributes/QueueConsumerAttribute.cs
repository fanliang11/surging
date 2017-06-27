using System;
using System.Collections.Generic;
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

        private string _queueName { get; set; }

        public QueueConsumerAttribute(string queueName)
        {
            _queueName = queueName;
        }

    }
}
