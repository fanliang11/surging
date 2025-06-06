using DotNetty.Buffers;
using Newtonsoft.Json;
using Surging.Core.CPlatform.Codecs.Core.Implementation;
using Surging.Core.CPlatform.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks; 
using  System.Xml;

namespace Surging.Core.DeviceGateway.Runtime.Device.Implementation
{
    public class PayloadDecoder
    { 

        public static byte[] GetBytes(IByteBuffer payload)
        {
            var result = new byte[payload.ReadableBytes];
            payload.ReadBytes(result);
            return result;
        }

        public static byte[] GetBytes(IByteBuffer payload, int offset, int len)
        {
            var result = new byte[len - offset];
            payload.ReadBytes(result, offset, len);
            return result;
        }

        public static JsonObject PayloadAsJson(IByteBuffer payload)
        {
            return JsonConvert.DeserializeObject<JsonObject>(Encoding.UTF8.GetString(GetBytes(payload)));
        }

        public static JsonArray PayloadAsJsonArray(IByteBuffer payload)
        {
            return JsonConvert.DeserializeObject<JsonArray>(Encoding.UTF8.GetString(GetBytes(payload)));
        }

        public static string PayloadAsString(IByteBuffer payload)
        {
            return payload.ToString(Encoding.UTF8);
        }

        public static string PayloadAsXml(IByteBuffer payload)
        {
            using (var stream = new MemoryStream(GetBytes(payload)))
            {
                var xmlDoc = new XmlTextReader(stream);
                return   xmlDoc.ReadContentAsString();
            }
        }

        public static object Read(IByteBuffer payload, PayloadType type)
        {
            object result = default;
            switch(type)
            {
                case PayloadType.Json:
                    result= PayloadAsJson(payload);
                    break;
                case PayloadType.String:
                    result = PayloadAsString(payload);
                    break;

                case PayloadType.Xml:
                      result = PayloadAsXml(payload);
                    break;
            }
            return result;
        }
    }
}
