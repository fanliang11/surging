using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.Codecs
{
    public class FixedLengthFrameDecoder : ByteToMessageDecoder
    {
        private readonly int frameLength;
        public FixedLengthFrameDecoder(int frameLength)
        {
            if (frameLength <= 0)
                throw new ArgumentOutOfRangeException(nameof(frameLength));
            this.frameLength = frameLength;
        }
        protected override void Decode(IChannelHandlerContext ctx, IByteBuffer input, List<object> output)
        { 
            object decoded = this.Decode(ctx, input);
            if (decoded != null)
                output.Add(decoded);
        }

        protected virtual object Decode(IChannelHandlerContext ctx, IByteBuffer buffer)
        {
            if (buffer.ReadableBytes < frameLength)
            {
                return null;
            }
            else
            {
                return buffer.ReadRetainedSlice(frameLength);
            }
        }
    }
}

