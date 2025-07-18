using DotNetty.Buffers;
using Surging.Core.CPlatform.Codecs.Core;
using Surging.Core.CPlatform.Codecs.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Codecs.Message
{
    public class MqttMessage: EncodedMessage
    {
        public string Topic { get; set; }

        public string ClientId { get; set; }

        public int QosLevel { get; set; }

        public IByteBuffer Payload { get; set; }

        public int MessageId { get; set; }

        public bool Will {  get; set; }

        public bool Dup {  get; set; }

        public bool Retain {  get; set; }
    }
}
