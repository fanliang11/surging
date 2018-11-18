using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Protocol.Mqtt.Internal.Enums
{
    public class PublishMessage: MessageWithId
    {
        public override MessageType MessageType => MessageType.PUBLISH;

        public IChannelHandlerContext Channel { get; set; }

         public byte[] ByteBuf { get; set; }

        public override bool RetainRequested { get; set; }

        public override int Qos { get; set;}

        public string TopicName { get; set; }
    }
}
