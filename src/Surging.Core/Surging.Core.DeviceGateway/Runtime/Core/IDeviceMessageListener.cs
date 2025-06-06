using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport;
using Surging.Core.DeviceGateway.Runtime.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core
{  
    /// <summary>
    /// 接受到消息的委托。
    /// </summary>
    /// <param name="sender">消息发送者。</param>
    /// <param name="message">接收到的消息。</param>
    public delegate Task DeviceReceivedDelegate(IDeviceMessageSender sender, IDeviceMessage message);
     
    /// <summary>
    /// 一个抽象的消息监听者。
    /// </summary>
    public interface IDeviceMessageListener
    {
        /// <summary>
        /// 接收到消息的事件。
        /// </summary>
        event DeviceReceivedDelegate Received;

        /// <summary>
        /// 触发接收到消息事件。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="message">接收到的消息。</param>
        /// <returns>一个任务。</returns>
        Task OnReceived(IDeviceMessageSender sender, IDeviceMessage message);
    } 
}
