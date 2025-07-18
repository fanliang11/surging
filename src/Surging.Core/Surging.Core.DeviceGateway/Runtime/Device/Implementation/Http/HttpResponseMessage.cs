using DotNetty.Buffers;
using Microsoft.Extensions.Primitives;
using Surging.Core.CPlatform.Codecs.Core;
using Surging.Core.CPlatform.Codecs.Message;
using Surging.Core.DeviceGateway.Runtime.Core.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Implementation.Http
{
    public abstract class HttpResponseMessage: EncodedMessage
    {
       public int Status { get; internal set; }

        public string ContentType { get; internal set; }

        public  List<Header> Headers { get; internal set; }=new List<Header>();


        public string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("HTTP").AppendFormat($" {0}",Status).Append(Enum.GetName(typeof(HttpStatus), Status)).Append("\n");
            foreach (Header header in Headers)
            {
                builder
                        .Append(header.Name).Append(": ").Append(string.Join(",", header.Value))
                        .Append("\n");
            }
            if (ContentType!=null)
            {
                builder.Append("Content-Type: ").Append(ContentType).Append("\n");
            }
            var payload = this.Payload;
            var len = payload.ReadableBytes;
            if (len == 0)
            {
                return builder.ToString();
            }
            builder.Append("\n");
            if (ByteBufferUtil.IsText(payload, 0, len,Encoding.UTF8))
            {
                builder.Append(payload.ToString(Encoding.UTF8));
            }
            else
            {
                ByteBufferUtil.AppendPrettyHexDump(builder, payload);
            }
            return builder.ToString();
        }
    }
}
