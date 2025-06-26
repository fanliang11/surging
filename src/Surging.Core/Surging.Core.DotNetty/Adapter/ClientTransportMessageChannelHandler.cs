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

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var buffer = (IByteBuffer)message;
            var data = new Memory<byte>(new byte[buffer.ReadableBytes]);
            try
            {
                buffer.ReadBytes(data);
                var transportMessage = _transportMessageDecoder.Decode(data);
                context.FireChannelRead(transportMessage);
            }
            finally
            {
                ReferenceCountUtil.Release(buffer);
                message = null;
            }
        }

        protected override void ChannelRead0(IChannelHandlerContext ctx, IByteBuffer msg)
        {
        }

        #endregion Overrides of ChannelHandlerAdapter
    }
}