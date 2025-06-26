using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Surging.Core.Protocol.Tcp.Runtime.Implementation
{
    public class DefaultDeviceProvider : IDeviceProvider
    {
        private readonly ConcurrentDictionary<string, IChannelHandlerContext> deviceItems = new ConcurrentDictionary<string, IChannelHandlerContext>();
        public bool IsConnected(string clientId)
        {
            var result = false;
            var device = deviceItems.GetValueOrDefault(clientId);
            if (device != null)
            {
                result = device.Channel.Active;
            }
            return result;
        }

        public bool Register(IChannelHandlerContext channelHandler)
        {
            return deviceItems.TryAdd(channelHandler.Channel.Id.AsLongText(), channelHandler);
        }

        

        public async Task SendClientMessage(string clientId, object message)
        {
            var device = deviceItems.GetValueOrDefault(clientId);
            if (device != null)
            {
                await device.WriteAndFlushAsync(message);
            }

        }

        public bool Unregister(IChannelHandlerContext channelHandler)
        {
            return deviceItems.Remove(channelHandler.Channel.Id.AsLongText(), out IChannelHandlerContext handlerContext);
        }

        public bool Unregister(string clientId)
        {
            return deviceItems.Remove(clientId, out IChannelHandlerContext handlerContext);
        }
    }
}
