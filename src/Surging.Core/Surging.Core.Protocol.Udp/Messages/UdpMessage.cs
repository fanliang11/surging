using DotNetty.Buffers;
using Surging.Core.CPlatform.Codecs.Core.Implementation;
using Surging.Core.CPlatform.Codecs.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Udp.Messages
{
    public class UdpMessage : EncodedMessage
    {

        public UdpMessage(IByteBuffer payload) : this(payload, MessagePayloadType.JSON)
        {
        }

        public UdpMessage(IByteBuffer payload, MessagePayloadType messagePayloadType) : base(payload, messagePayloadType)
        {
        }

        public override string ToString()
        {

            StringBuilder builder = new StringBuilder();

            if (ByteBufferUtil.IsText(Payload, 0, Payload.ReadableBytes, Encoding.UTF8))
            {
                builder.Append(Payload.ToString(Encoding.UTF8));
            }
            else
            {
                ByteBufferUtil.AppendPrettyHexDump(builder, Payload);
            }
            return builder.ToString();
        }

    }
}

