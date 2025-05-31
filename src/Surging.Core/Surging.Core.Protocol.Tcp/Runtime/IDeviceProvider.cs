using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.Runtime
{
    public interface IDeviceProvider
    {
        bool Register(IChannelHandlerContext channelHandler);

        bool Unregister(IChannelHandlerContext channelHandlerContext);

        bool Unregister(string clientId);

        bool IsConnected(string clientId);

        Task SendClientMessage(string clientId,object message);
         
    }
}
