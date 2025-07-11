using DotNetty.Buffers;
using Surging.Core.CPlatform.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.Runtime
{
    public interface ITcpMessageSender: IDeviceMessageSender
    {
        Task SendAsync(object message);
        Task SendAndFlushAsync(object message);

        Task SendAsync(object message, Encoding encoding);
         
        Task SendAndFlushAsync(object message, Encoding encoding);

        Task SendAndFlushAsync(IByteBuffer buffer);

        Task SendAsync(IByteBuffer buffer);
    }
}
