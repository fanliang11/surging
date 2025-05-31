using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Codecs.Message
{
    public class EmptyMessage : EncodedMessage
    {
        public static readonly EmptyMessage INSTANCE = new EmptyMessage();
        public IByteBuffer Payload { get; set; } = Unpooled.WrappedBuffer(new byte[0]);

        public override string ToString()
        {
            return "empty message";
        }
    }
}
