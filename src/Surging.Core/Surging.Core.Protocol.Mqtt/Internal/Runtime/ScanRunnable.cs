using Surging.Core.Protocol.Mqtt.Internal.Enums;
using Surging.Core.Protocol.Mqtt.Internal.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Surging.Core.Protocol.Mqtt.Internal.Runtime
{
    /// <summary>
    /// Defines the <see cref="ScanRunnable" />
    /// </summary>
    public abstract class ScanRunnable : Runnable
    {
        #region 字段

        /// <summary>
        /// Defines the _queue
        /// </summary>
        private ConcurrentQueue<SendMqttMessage> _queue = new ConcurrentQueue<SendMqttMessage>();

        #endregion 字段

        #region 方法

        /// <summary>
        /// The Enqueue
        /// </summary>
        /// <param name="ts">The ts<see cref="List{SendMqttMessage}"/></param>
        public void Enqueue(List<SendMqttMessage> ts)
        {
            ts.ForEach(message => _queue.Enqueue(message));
        }

        /// <summary>
        /// The Enqueue
        /// </summary>
        /// <param name="t">The t<see cref="SendMqttMessage"/></param>
        public void Enqueue(SendMqttMessage t)
        {
            _queue.Enqueue(t);
        }

        /// <summary>
        /// The Execute
        /// </summary>
        /// <param name="poll">The poll<see cref="SendMqttMessage"/></param>
        public abstract void Execute(SendMqttMessage poll);

        /// <summary>
        /// The Run
        /// </summary>
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

        #endregion 方法
    }
}