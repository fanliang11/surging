using DotNetty.Buffers;
using Surging.Core.CPlatform.Codecs.Core.Implementation;
using System.Text;

namespace Surging.Core.CPlatform.Codecs.Message
{
    internal class SimpleEncodedMessage : EncodedMessage
    {

        private readonly IByteBuffer _payload;

        private readonly MessagePayloadType _payloadType;

        public SimpleEncodedMessage(IByteBuffer payload, MessagePayloadType payloadType)
        {
            _payload = payload;
            _payloadType = payloadType;
        }

        public static SimpleEncodedMessage Instance(IByteBuffer byteBuf, MessagePayloadType payloadType)
        {
            return new SimpleEncodedMessage(byteBuf, payloadType);
        }

        public override string ToString()
        {

            StringBuilder builder = new StringBuilder();

            if (ByteBufferUtil.IsText(_payload, 0, _payload.ReadableBytes, Encoding.UTF8))
            {
                builder.Append(_payload.ToString(Encoding.UTF8));
            }
            else
            {
                ByteBufferUtil.AppendPrettyHexDump(builder, _payload);
            }
            return builder.ToString();
        }
    }
}
