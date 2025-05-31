
using DotNetty.Buffers;
using Surging.Core.CPlatform.Codecs.Core.Implementation;
using Surging.Core.CPlatform.Codecs.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Codecs.Core
{
    public interface IEncodedMessage
    {
        IByteBuffer Payload { get; set; }
        string PayloadAsString();
        JsonObject PayloadAsJson();

        JsonArray PayloadAsJsonArray();

        byte[] GetBytes();

        byte[] GetBytes(int offset, int len);



        MessagePayloadType PayloadType { get; set; }


        EmptyMessage Empty();


        IEncodedMessage Simple(IByteBuffer data);
    }

}
