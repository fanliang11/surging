using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.DotNetty.Adapter
{
    class TransportMessageChannelHandlerAdapter : ChannelHandlerAdapter
    {
        private readonly ITransportMessageDecoder _transportMessageDecoder;

        public TransportMessageChannelHandlerAdapter(ITransportMessageDecoder transportMessageDecoder)
        {
            _transportMessageDecoder = transportMessageDecoder;
        }

        #region Overrides of ChannelHandlerAdapter

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IFullHttpRequest request)
            {
                try
                {
                    context.FireChannelRead(message);
                }
                finally
                {
                    ReferenceCountUtil.Release(message);
                }
            }
            else
            {
                if (message is IByteBuffer buffer)
                {
                    var data = new byte[buffer.ReadableBytes];
                    buffer.ReadBytes(data);
                    var transportMessage = _transportMessageDecoder.Decode(data);

                    context.FireChannelRead(transportMessage);
                    ReferenceCountUtil.Release(buffer);
                }
                else
                    context.FireChannelRead(message);
            }
        }

        #endregion Overrides of ChannelHandlerAdapter
    }
}