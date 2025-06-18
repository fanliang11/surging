using DotNetty.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.Messages.Codec
{
    public class EncodedMessage
    {
        public IByteBuffer Payload  { get; set; }
    }
}
