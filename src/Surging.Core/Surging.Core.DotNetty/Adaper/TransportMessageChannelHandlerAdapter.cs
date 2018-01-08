using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.DotNetty.Adaper
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
            var buffer = (IByteBuffer)message;
            try
            {
                try
                {
                    var data = new byte[buffer.ReadableBytes];
                    buffer.ReadBytes(data); 
                    var transportMessage = _transportMessageDecoder.Decode(data);
                    context.FireChannelRead(transportMessage);
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
            finally { buffer.Release(); }
        }

        #endregion Overrides of ChannelHandlerAdapter
    }
}