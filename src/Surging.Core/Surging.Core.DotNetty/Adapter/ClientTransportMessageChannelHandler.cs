using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DotNetty.Adapter
{
    public class ClientTransportMessageChannelHandler : SimpleChannelInboundHandler2<IByteBuffer>
    {
        private readonly ITransportMessageDecoder _transportMessageDecoder;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ClientTransportMessageChannelHandler(ITransportMessageDecoder transportMessageDecoder)
        {
            _transportMessageDecoder = transportMessageDecoder;
        }

        #region Overrides of ChannelHandlerAdapter

         
        protected override void ChannelRead0(IChannelHandlerContext ctx, IByteBuffer msg)
        {
            var data = new Memory<byte>(new byte[msg.ReadableBytes]); 
            msg.ReadBytes(data);
            var transportMessage = _transportMessageDecoder.Decode(data);
            ctx.FireChannelRead(transportMessage);
        }
         
        #endregion Overrides of ChannelHandlerAdapter
    }
}