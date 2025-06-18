using DotNetty.Buffers;
using Newtonsoft.Json;
using Surging.Core.CPlatform.Codecs.Core;
using Surging.Core.CPlatform.Codecs.Core.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Codecs.Message
{
    public abstract class EncodedMessage : IEncodedMessage
    {
        public IByteBuffer Payload { get; set; } = Unpooled.WrappedBuffer(new byte[0]);
        public MessagePayloadType PayloadType { get; set; } = MessagePayloadType.JSON;

        public EncodedMessage() { }
        public EncodedMessage(IByteBuffer paload, MessagePayloadType payloadType)
        {
            Payload = paload;
            PayloadType = payloadType;
        }

        public EmptyMessage Empty()
        {
            return EmptyMessage.INSTANCE;
        }

        public byte[] GetBytes()
        {
            var result = new byte[Payload.ReadableBytes];
            Payload.ReadBytes(result);
            return result;
        }

        public byte[] GetBytes(int offset, int len)
        {
            var result = new byte[len - offset];
            Payload.ReadBytes(result, offset, len);
            return result;
        }

        public JsonObject PayloadAsJson()
        {
            return JsonConvert.DeserializeObject<JsonObject>(Encoding.UTF8.GetString(GetBytes()));
        }

        public JsonArray PayloadAsJsonArray()
        {
            return JsonConvert.DeserializeObject<JsonArray>(Encoding.UTF8.GetString(GetBytes()));
        }

        public string PayloadAsString()
        {
            return Payload.ToString(Encoding.UTF8);
        }

        public IEncodedMessage Simple(IByteBuffer data)
        {
            return SimpleEncodedMessage.Instance(data, MessagePayloadType.BINARY);
        }
    }
}
