using DotNetty.Buffers;
using DotNetty.Codecs.DNS.Messages;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace DotNetty.Codecs.DNS
{
    /// <summary>
    /// Defines the <see cref="TcpDnsQueryEncoder" />
    /// </summary>
    public sealed class TcpDnsQueryEncoder : MessageToByteEncoder<IDnsQuery>
    {
        #region 字段

        /// <summary>
        /// Defines the encoder
        /// </summary>
        private readonly DnsQueryEncoder encoder;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpDnsQueryEncoder"/> class.
        /// </summary>
        public TcpDnsQueryEncoder() : this(new DefaultDnsRecordEncoder())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpDnsQueryEncoder"/> class.
        /// </summary>
        /// <param name="recordEncoder">The recordEncoder<see cref="IDnsRecordEncoder"/></param>
        public TcpDnsQueryEncoder(IDnsRecordEncoder recordEncoder)
        {
            this.encoder = new DnsQueryEncoder(recordEncoder);
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The AllocateBuffer
        /// </summary>
        /// <param name="ctx">The ctx<see cref="IChannelHandlerContext"/></param>
        /// <returns>The <see cref="IByteBuffer"/></returns>
        protected override IByteBuffer AllocateBuffer(IChannelHandlerContext ctx)
        {
            return ctx.Allocator.Buffer(1024);
        }

        /// <summary>
        /// The Encode
        /// </summary>
        /// <param name="context">The context<see cref="IChannelHandlerContext"/></param>
        /// <param name="message">The message<see cref="IDnsQuery"/></param>
        /// <param name="output">The output<see cref="IByteBuffer"/></param>
        protected override void Encode(IChannelHandlerContext context, IDnsQuery message, IByteBuffer output)
        {
            output.SetWriterIndex(output.WriterIndex + 2);
            encoder.Encode(message, output);
            output.SetShort(0, output.ReadableBytes - 2);
        }

        #endregion 方法
    }
}