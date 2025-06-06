using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Surging.Core.DotNetty.Adapter
{
    class TransportMessageChannelHandlerAdapter : ByteToMessageDecoder
    {
        private readonly ITransportMessageDecoder _transportMessageDecoder;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public TransportMessageChannelHandlerAdapter(ITransportMessageDecoder transportMessageDecoder)
        {
            SetCumulator(ByteToMessageDecoder.CompositionCumulation);
            _transportMessageDecoder = transportMessageDecoder;
        }

        #region Overrides of ChannelHandlerAdapter
         
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            Span<byte> data = stackalloc byte[input.ReadableBytes];
            input.ReadBytes(data);
            var transportMessage = _transportMessageDecoder.Decode(data.ToArray());
            output.Add(transportMessage);
        }

        protected override void DecodeLast(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            base.HandlerRemoved(context);
        }


        #endregion Overrides of ChannelHandlerAdapter
    }
}