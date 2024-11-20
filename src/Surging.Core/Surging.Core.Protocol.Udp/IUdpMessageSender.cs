using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using Surging.Core.CPlatform.Transport;
using Surging.Core.Protocol.Udp.Runtime.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Udp
{
    public interface IUdpMessageSender: IMessageSender
    {
        T GetAndSet<T>(AttributeKey<T> attributeKey, T obj) where T : class;
        T Get<T>(AttributeKey<T> attributeKey) where T : class;
        UdpClient GetClient();
        Task SendAsync(string value, Encoding encoding);

        Task SendAndFlushAsync(string value, Encoding encoding);

        Task SendAsync(object message);
        Task SendAndFlushAsync(object message);


        Task SendAndFlushAsync(IByteBuffer buffer);

        Task SendAsync(IByteBuffer buffer);
    }
}
