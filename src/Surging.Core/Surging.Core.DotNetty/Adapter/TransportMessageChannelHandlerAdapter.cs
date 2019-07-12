using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.DotNetty.Adapter
{
    /// <summary>
    /// Defines the <see cref="TransportMessageChannelHandlerAdapter" />
    /// </summary>
    internal class TransportMessageChannelHandlerAdapter : ChannelHandlerAdapter
    {
        #region 字段

        /// <summary>
        /// Defines the _transportMessageDecoder
        /// </summary>
        private readonly ITransportMessageDecoder _transportMessageDecoder;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="TransportMessageChannelHandlerAdapter"/> class.
        /// </summary>
        /// <param name="transportMessageDecoder">The transportMessageDecoder<see cref="ITransportMessageDecoder"/></param>
        public TransportMessageChannelHandlerAdapter(ITransportMessageDecoder transportMessageDecoder)
        {
            _transportMessageDecoder = transportMessageDecoder;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The ChannelRead
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="message">The message<see cref="object"/></param>
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var buffer = (IByteBuffer)message;
            var data = new byte[buffer.ReadableBytes];
            buffer.ReadBytes(data);
            var transportMessage = _transportMessageDecoder.Decode(data);
            context.FireChannelRead(transportMessage);
            ReferenceCountUtil.Release(buffer);
        }

        #endregion 方法
    }
}