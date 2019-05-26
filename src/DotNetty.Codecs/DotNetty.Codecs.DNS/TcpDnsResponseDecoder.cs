using DotNetty.Buffers;
using DotNetty.Codecs.DNS.Messages;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DotNetty.Codecs.DNS
{
    public  class TcpDnsResponseDecoder : LengthFieldBasedFrameDecoder
    {
        private readonly DnsResponseDecoder<EndPoint> responseDecoder;

        public TcpDnsResponseDecoder():this(new DefaultDnsRecordDecoder(), 64 * 1024)
        {

        }

        public TcpDnsResponseDecoder(IDnsRecordDecoder recordDecoder, int maxFrameLength) : base(maxFrameLength, 0, 2, 0, 2)
        {

            this.responseDecoder = new DnsResponseDecoder<EndPoint>(recordDecoder);
        }

        protected override Object Decode(IChannelHandlerContext ctx, IByteBuffer buffer)
        {
            var frame = (IByteBuffer)base.Decode(ctx, buffer);
            if (frame == null) {
                return null;
            }

            try {
                return responseDecoder.Decode(ctx.Channel.RemoteAddress, ctx.Channel.LocalAddress, frame.Slice());
            } finally {
                frame.Release();
            }
        }


        protected override IByteBuffer ExtractFrame(IChannelHandlerContext ctx, IByteBuffer buffer, int index, int length)
        {
            return buffer.Copy(index, length);
        }
    }

}
