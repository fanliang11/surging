using Surging.Core.Protocol.Mqtt.Internal.Enums;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime
{
    public abstract class ScanRunnable : Runnable
    {
        private ConcurrentQueue<SendMqttMessage> _queue = new ConcurrentQueue<SendMqttMessage>();
        public void Enqueue(SendMqttMessage t)
        {
             _queue.Enqueue(t);
        }

        public void Enqueue(List<SendMqttMessage> ts)
        {
            ts.ForEach(message=> _queue.Enqueue(message));
        }

        public override void Run()
        {
            if (!_queue.IsEmpty)
            {
                List<SendMqttMessage> list = new List<SendMqttMessage>();
                for (; (_queue.TryDequeue(out SendMqttMessage poll));)
                {
                    if (poll.ConfirmStatus != ConfirmStatus.COMPLETE)
                    {
                        list.Add(poll);
                        Execute(poll);
                    }
                    break;
                }
            }
        }

        public abstract void Execute(SendMqttMessage poll);
    }
}
