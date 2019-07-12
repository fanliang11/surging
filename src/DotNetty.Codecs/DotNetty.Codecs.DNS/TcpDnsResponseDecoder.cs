using DotNetty.Buffers;
using DotNetty.Codecs.DNS.Messages;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DotNetty.Codecs.DNS
{
    /// <summary>
    /// Defines the <see cref="TcpDnsResponseDecoder" />
    /// </summary>
    public class TcpDnsResponseDecoder : LengthFieldBasedFrameDecoder
    {
        #region 字段

        /// <summary>
        /// Defines the responseDecoder
        /// </summary>
        private readonly DnsResponseDecoder<EndPoint> responseDecoder;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpDnsResponseDecoder"/> class.
        /// </summary>
        public TcpDnsResponseDecoder() : this(new DefaultDnsRecordDecoder(), 64 * 1024)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpDnsResponseDecoder"/> class.
        /// </summary>
        /// <param name="recordDecoder">The recordDecoder<see cref="IDnsRecordDecoder"/></param>
        /// <param name="maxFrameLength">The maxFrameLength<see cref="int"/></param>
        public TcpDnsResponseDecoder(IDnsRecordDecoder recordDecoder, int maxFrameLength) : base(maxFrameLength, 0, 2, 0, 2)
        {
            this.responseDecoder = new DnsResponseDecoder<EndPoint>(recordDecoder);
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The Decode
        /// </summary>
        /// <param name="ctx">The ctx<see cref="IChannelHandlerContext"/></param>
        /// <param name="buffer">The buffer<see cref="IByteBuffer"/></param>
        /// <returns>The <see cref="Object"/></returns>
        protected override Object Decode(IChannelHandlerContext ctx, IByteBuffer buffer)
        {
            var frame = (IByteBuffer)base.Decode(ctx, buffer);
            if (frame == null)
            {
                return null;
            }

            try
            {
                return responseDecoder.Decode(ctx.Channel.RemoteAddress, ctx.Channel.LocalAddress, frame.Slice());
            }
            finally
            {
                frame.Release();
            }
        }

        /// <summary>
        /// The ExtractFrame
        /// </summary>
        /// <param name="ctx">The ctx<see cref="IChannelHandlerContext"/></param>
        /// <param name="buffer">The buffer<see cref="IByteBuffer"/></param>
        /// <param name="index">The index<see cref="int"/></param>
        /// <param name="length">The length<see cref="int"/></param>
        /// <returns>The <see cref="IByteBuffer"/></returns>
        protected override IByteBuffer ExtractFrame(IChannelHandlerContext ctx, IByteBuffer buffer, int index, int length)
        {
            return buffer.Copy(index, length);
        }

        #endregion 方法
    }
}