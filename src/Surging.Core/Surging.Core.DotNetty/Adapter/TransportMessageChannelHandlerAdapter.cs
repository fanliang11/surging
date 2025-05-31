﻿using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Surging.Core.DotNetty.Adapter
{
    class TransportMessageChannelHandlerAdapter : ChannelHandlerAdapter
    {
        private readonly ITransportMessageDecoder _transportMessageDecoder;

        [MethodImpl(MethodImplOptions.NoInlining)]
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
                Span<byte> data = stackalloc byte[buffer.ReadableBytes];
                buffer.ReadBytes(data);
                var transportMessage = _transportMessageDecoder.Decode(data.ToArray());
                context.FireChannelRead(transportMessage);
            }
            finally
            {
                ReferenceCountUtil.Release(buffer); 
                message = null;
            }
        }

        #endregion Overrides of ChannelHandlerAdapter
    }
}