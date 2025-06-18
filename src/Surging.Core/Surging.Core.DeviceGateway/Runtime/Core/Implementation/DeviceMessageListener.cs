using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using Surging.Core.DeviceGateway.Runtime.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Implementation
{
    public class DeviceMessageListener: IDeviceMessageListener
    {
        public event DeviceReceivedDelegate Received;

        /// <summary>
        /// 触发接收到消息事件。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="message">接收到的消息。</param>
        /// <returns>一个任务。</returns>
        public async Task OnReceived(IDeviceMessageSender sender, IDeviceMessage message)
        {
            if (Received == null)
                return;
            await Received(sender, message);
        }
    }
}
