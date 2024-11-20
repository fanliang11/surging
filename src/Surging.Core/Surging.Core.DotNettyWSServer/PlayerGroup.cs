using DotNetty.Buffers;
using DotNetty.Codecs.Http.WebSockets;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DotNettyWSServer
{
    public class PlayerGroup
    {
        public static IChannelGroup ChannelGroup { get; set; }

         public static void AddChannel(IChannel channel)
        {
            ChannelGroup.Add(channel);
        }

        public static void RemoveChannel(IChannel channel)
        {
            ChannelGroup.Remove(channel);
        }

        public static async Task BroadCast(IByteBuffer message)
        {
            if (ChannelGroup == null) return;

            BinaryWebSocketFrame frame = new BinaryWebSocketFrame(message);
            message.Retain();
            await ChannelGroup.WriteAndFlushAsync(frame);
        }

        public static async Task Destory()
        {
            if (ChannelGroup == null) return;
            await ChannelGroup.CloseAsync();
        }
    }
}
