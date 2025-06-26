using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DotNetty.Adapter
{
    public class TransportMessageHandlerEncoder : MessageToByteEncoder<TransportMessage>
    {
        private readonly ITransportMessageEncoder _transportMessageEncoder;

        public TransportMessageHandlerEncoder(ITransportMessageEncoder transportMessageEncoder)
        {
            _transportMessageEncoder = transportMessageEncoder;
        }
        protected override void Encode(IChannelHandlerContext context, TransportMessage message, IByteBuffer output)
        {
            try
            {
                output.WriteBytes(_transportMessageEncoder.Encode(message));
            }
            finally
            {
                message = null;
            }
        }
    }
}
