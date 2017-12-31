using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.DotNetty
{
    class QuoteOfTheMomentClientHandler : SimpleChannelInboundHandler<DatagramPacket>
    {
        protected override void ChannelRead0(IChannelHandlerContext ctx, DatagramPacket packet)
        {
            Console.WriteLine($"Client Received => {packet}");

            if (!packet.Content.IsReadable())
            {
                return;
            }

            string message = packet.Content.ToString(Encoding.UTF8);
            if (!message.StartsWith("QOTM: "))
            {
                return;
            }

            Console.WriteLine($"Quote of the Moment: {message.Substring(6)}");
            ctx.CloseAsync();
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            context.CloseAsync();
        }
    }
}
