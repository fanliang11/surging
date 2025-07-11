using DotNetty.Buffers;
using Microsoft.Extensions.Primitives;
using Surging.Core.CPlatform.Codecs.Core.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Codecs.Message
{
    public class TcpMessage : EncodedMessage
    {  

        public TcpMessage(IByteBuffer payload):this(payload,MessagePayloadType.JSON)
        { 
        }

        public TcpMessage(IByteBuffer payload,MessagePayloadType messagePayloadType):base(payload, messagePayloadType)
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
 